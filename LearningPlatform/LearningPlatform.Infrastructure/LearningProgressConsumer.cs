using Azure.Messaging.ServiceBus;
using LearningPlatform.BusinessLogic.LearningProgress;
using LearningPlatform.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LearningPlatform.Infrastructure
{
	public class LearningProgressConsumer : BackgroundService
	{
		private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

		private readonly ILogger<LearningProgressConsumer> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;     // IServiceScopeFactory is used to create a scope for the scoped services (like DbContext) that we need to use in this background service
		private readonly ServiceBusClient _serviceBusClient;
		private readonly string _topicName;
		private readonly string _subscriptionName;

		public LearningProgressConsumer(
			ILogger<LearningProgressConsumer> logger,
			IServiceScopeFactory serviceScopeFactory,
			ServiceBusClient serviceBusClient,
			string topicName,
			string subscriptionName)
		{
			this._logger = logger;
			this._serviceScopeFactory = serviceScopeFactory;
			this._serviceBusClient = serviceBusClient;
			this._topicName = topicName;
			this._subscriptionName = subscriptionName;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			this._logger.LogWarning("Starting LearningProgressConsumer background service.");

			var options = new ServiceBusProcessorOptions
			{
				AutoCompleteMessages = false,
				MaxConcurrentCalls = 1,
			};

			await using var processor = this._serviceBusClient.CreateProcessor(this._topicName, this._subscriptionName, options);

			processor.ProcessMessageAsync += this.ProcessMessageAsync;
			processor.ProcessErrorAsync += this.ProcessErrorAsync;

			await processor.StartProcessingAsync(stoppingToken);

			try
			{
				await Task.Delay(Timeout.Infinite, stoppingToken);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
			}
			finally
			{
				await processor.StopProcessingAsync();
			}
		}

		private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
		{
			// Process the delivered Service Bus message
			var enrollmentCreatedEvent = JsonSerializer.Deserialize<EnrollmentCreatedEvent>(args.Message.Body.ToString(), _jsonSerializerOptions)
				?? throw new InvalidOperationException("Learning progress consumer is unable to deserialize EnrollmentCreatedEvent.");

			this._logger.LogWarning("Learning progress consumer processing EnrollmentCreatedEvent SB message. EventId={EventId}, LearnerId={LearnerId}, CourseId={CourseId}",
				enrollmentCreatedEvent.EventId,
				enrollmentCreatedEvent.LearnerId,
				enrollmentCreatedEvent.CourseId);

			using var scope = this._serviceScopeFactory.CreateScope();

			// Get the scoped service (LearningProgressService) from the service provider to run the business logic.
			var learningProgressService = scope.ServiceProvider.GetRequiredService<ILearningProgressService>();

			// Invoke the idempotent business operation; the business/data layers ensure that a redelivered Service Bus message does not repeat the business effect.
			var isDuplicate = await learningProgressService.ProcessEnrollmentCreated(enrollmentCreatedEvent, args.CancellationToken);

			this._logger.LogWarning("Processed EnrollmentCreated message. MessageId={MessageId}, DeliveryCount={DeliveryCount}, IsDuplicate={IsDuplicate}",
				args.Message.MessageId,
				args.Message.DeliveryCount,
				isDuplicate);

			// Simulate a crash after the SQL commit on the first delivery, but before the Service Bus Complete, to test the idempotency of the consumer for the subsequent delivery attempt.
			if (args.Message.DeliveryCount == 1)
			{
				// Will force the message to be redelivered, which is why the consumer must be idempotent (i.e., it must not create duplicate learning progress records for the same enrollment event)
				throw new InvalidOperationException("Simulated crash after SQL commit (only on first delivery), before Service Bus Complete.");
			}

			// Complete the delivered message after processing (without this, the delivered message will be redelivered after the lock expires, or if/when the processor abandons/releases the message).
			await args.CompleteMessageAsync(args.Message, args.CancellationToken);

			this._logger.LogWarning("Learning progress consumer completed EnrollmentCreatedEvent SB message.");
		}

		private Task ProcessErrorAsync(ProcessErrorEventArgs args)
		{
			this._logger.LogError(args.Exception, "Learning progress consumer encountered an error while processing a Service Bus message.");

			return Task.CompletedTask;
		}
	}
}
