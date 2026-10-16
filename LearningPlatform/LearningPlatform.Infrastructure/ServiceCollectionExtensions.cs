using Azure.Identity;
using Azure.Messaging.ServiceBus;
using LearningPlatform.BusinessLogic.LearningProgress;
using LearningPlatform.BusinessLogic.Outbox;
using LearningPlatform.DataLayer;
using LearningPlatform.DataLayer.LearningProgress;
using LearningPlatform.DataLayer.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LearningPlatform.Infrastructure
{
	public static class ServiceCollectionExtensions
	{
		// Register all LearningPlatform infrastructure services.
		public static IServiceCollection AddLearningPlatformInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			// This preserves the existing single-process hosting model, where the Enrollment API
			// also runs both the OutboxDispatcher and LearningProgressConsumer background services (i.e., monolithic architecture).
			services.AddLearningPlatformServiceBus(configuration);
			services.AddOutboxDispatcher(configuration);
			services.AddLearningProgressConsumer(configuration);

			return services;
		}

		// Register the common Service Bus infrastructure used by both the outbox publisher and the learning progress consumer.
		public static IServiceCollection AddLearningPlatformServiceBus(this IServiceCollection services, IConfiguration configuration)
		{
			// Get the Service Bus namespace from configuration (appsettings.json, environment variables, etc.)
			var fullyQualifiedNamespace = configuration["ServiceBus:FullyQualifiedNamespace"] ?? throw new InvalidOperationException("Service Bus fully qualified namespace is not configured.");

			// Register the ServiceBusClient as a singleton.
			// DefaultAzureCredential allows the same application code to authenticate differently
			// depending on the hosting environment (developer identity, service principal,
			// managed identity, workload identity, etc.).
			services.AddSingleton<ServiceBusClient>(new ServiceBusClient(fullyQualifiedNamespace, new DefaultAzureCredential()));

			return services;
		}

		// Register the infrastructure required to dispatch outbox messages to Service Bus.
		public static IServiceCollection AddOutboxDispatcher(this IServiceCollection services, IConfiguration configuration)
		{
			// Get the enrollment-created topic from configuration.
			var enrollmentCreatedTopic = configuration["ServiceBus:EnrollmentCreatedTopic"] ?? throw new InvalidOperationException( "Enrollment-created topic is not configured.");

			// Register the ServiceBusSender for the enrollment-created topic as a singleton.
			services.AddSingleton<ServiceBusSender>(serviceProvider =>
			{
				var serviceBusClient = serviceProvider.GetRequiredService<ServiceBusClient>();

				return serviceBusClient.CreateSender(enrollmentCreatedTopic);
			});

			// Register the ServiceBusEnrollmentEventPublisher as a singleton, which implements IOutboxMessagePublisher.
			services.AddSingleton<IOutboxMessagePublisher, ServiceBusEnrollmentEventPublisher>();

			// Register the OutboxDispatcher as a hosted service, which will run in the background and dispatch unpublished outbox messages.
			services.AddHostedService<OutboxDispatcher>();

			return services;
		}

		// Register the infrastructure required to consume EnrollmentCreatedEvent messages and update learning progress.
		public static IServiceCollection AddLearningProgressConsumer(this IServiceCollection services, IConfiguration configuration)
		{
			// Get the Service Bus topic and subscription from configuration.
			var enrollmentCreatedTopic = configuration["ServiceBus:EnrollmentCreatedTopic"] ?? throw new InvalidOperationException("Enrollment-created topic is not configured.");
			var learningProgressSubscription = configuration["ServiceBus:LearningProgressSubscription"] ?? throw new InvalidOperationException("Learning-progress subscription is not configured.");

			// Register the LearningProgressConsumer as a hosted service, which will run in the background and consume learning progress messages from the Service Bus subscription.
			services.AddHostedService<LearningProgressConsumer>(serviceProvider =>
				new LearningProgressConsumer(
					serviceProvider.GetRequiredService<ILogger<LearningProgressConsumer>>(),
					serviceProvider.GetRequiredService<IServiceScopeFactory>(),
					serviceProvider.GetRequiredService<ServiceBusClient>(),
					enrollmentCreatedTopic,
					learningProgressSubscription));

			return services;
		}

		// Register all services required by the dedicated Outbox Worker host.
		public static IServiceCollection AddOutboxWorker(this IServiceCollection services, IConfiguration configuration)
		{
			// Configure the database context to use SQL Server with the connection string from appsettings.json
			// Note that this uses scoped lifetime, because the DbContext is not thread-safe and should not be shared across requests.
			services.AddDbContext<LearningPlatformDbContext>(options =>
				options.UseSqlServer(
					configuration.GetSection("ConnectionStrings")["LearningPlatform"]));

			// Register the repository and business service as scoped services because they ultimately depend on the scoped DbContext.
			services.AddScoped<IOutboxRepository, OutboxRepository>();
			services.AddScoped<IOutboxService, OutboxService>();

			// Register the common Service Bus infrastructure.
			services.AddLearningPlatformServiceBus(configuration);

			// Register the Service Bus sender, outbox publisher, and OutboxDispatcher hosted service.
			services.AddOutboxDispatcher(configuration);

			return services;
		}

		// Register all services required by the dedicated Learning Progress Worker host.
		public static IServiceCollection AddLearningProgressWorker(this IServiceCollection services, IConfiguration configuration)
		{
			// Register the reusable business and data services required to process learning progress events.
			services.AddLearningProgressProcessing(configuration);

			// Register the common Service Bus infrastructure.
			services.AddLearningPlatformServiceBus(configuration);

			// Register the LearningProgressConsumer hosted service.
			services.AddLearningProgressConsumer(configuration);

			return services;
		}


		// Register the business and data services required to process learning progress events.
		// These services are shared by both the dedicated Worker Service host and the Azure Function host.
		public static IServiceCollection AddLearningProgressProcessing(this IServiceCollection services, IConfiguration configuration)
		{
			// Configure the database context to use SQL Server with the connection string from appsettings.json
			// Note that this uses scoped lifetime, because the DbContext is not thread-safe and should not be shared across requests.
			services.AddDbContext<LearningPlatformDbContext>(options =>
				options.UseSqlServer(
					configuration.GetSection("ConnectionStrings")["LearningPlatform"]));

			// Register the repository and business service as scoped services because they ultimately depend on the scoped DbContext.
			services.AddScoped<ILearningProgressRepository, LearningProgressRepository>();
			services.AddScoped<ILearningProgressService, LearningProgressService>();

			return services;
		}


	}
}
