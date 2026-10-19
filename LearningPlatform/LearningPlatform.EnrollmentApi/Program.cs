using LearningPlatform.BusinessLogic.Enrollments;
using LearningPlatform.BusinessLogic.LearningProgress;
using LearningPlatform.BusinessLogic.Outbox;
using LearningPlatform.BusinessLogic.Recommendations;
using LearningPlatform.DataLayer;
using LearningPlatform.DataLayer.Enrollment;
using LearningPlatform.DataLayer.LearningProgress;
using LearningPlatform.DataLayer.Outbox;
using LearningPlatform.DataLayer.Recommendations;
using LearningPlatform.Infrastructure;
using LearningPlatform.Recommendations;
using LearningPlatform.Recommendations.Agent;
using LearningPlatform.Recommendations.Agent.LearningPlatform.Recommendations.Agent;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

namespace LearningPlatform.EnrollmentApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Get application configuration.

			var foundryEndpoint = builder.Configuration["Foundry:ProjectEndpoint"]
				?? throw new InvalidOperationException("Foundry:ProjectEndpoint is not configured.");

			var foundryModel = builder.Configuration["Foundry:Model"]
				?? throw new InvalidOperationException("Foundry:Model is not configured.");         
			
			// Add services to the container.

			builder.Services.AddControllers();

			// Currently not needed since we're presently serving the UI from the same origin as the web API (built to be served as static files)
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("ReactFrontEnd", policy =>
				{
					policy
						.WithOrigins("http://localhost:5173")
						.AllowAnyHeader()
						.AllowAnyMethod();
				});
			});

			builder.Services
				.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("Entra"));

			builder.Services.AddAuthorization();            
			
			// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
			builder.Services.AddOpenApi();

			// Configure the database context to use SQL Server with the connection string from appsettings.json.
			// Note that this uses scoped lifetime, because the DbContext is not thread-safe and should not be shared across requests.
			builder.Services.AddDbContext<LearningPlatformDbContext>(options =>
				options.UseSqlServer(
					builder.Configuration.GetSection("ConnectionStrings")["LearningPlatform"]));

			// Even though our services and repositories are stateless (and could therefore be registered as singletons), we use
			// scoped lifetime for them when they depend on other services that are scoped; namely, the DbContext.
			builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
			builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
			builder.Services.AddScoped<ILearningDataRepository, SqlLearningDataRepository>();
			builder.Services.AddScoped<IRecommendationApplicationService, RecommendationApplicationService>();
			builder.Services.AddScoped<IRecommendationAgentService>(serviceProvider => CreateRecommendationAgentService(serviceProvider, foundryEndpoint, foundryModel)); 
			builder.Services.AddScoped<RecommendationTools>();

			// These can be singletons because (a) they are stateless and (b) they do not depend on other scoped services (e.g.; DbContext).
			builder.Services.AddSingleton<ICourseRanker, WeightedCourseRanker>();
			builder.Services.AddSingleton<IEligibilityService, EligibilityService>();
			builder.Services.AddSingleton<IRecommendationService, RecommendationService>();

			// In the single-process hosting model (i.e., monolithic architecture), the API also runs the background services that dispatch outbox messages and consume learning progress messages.
			// In the decomposed hosting model (i.e., microservices architecture), those background services run in separate worker processes.
			var runBackgroundServices = builder.Configuration.GetValue("Hosting:RunBackgroundServices", defaultValue: true);
			if (runBackgroundServices)
			{
				// Register the business and data services required by the background services.
				builder.Services.AddScoped<ILearningProgressRepository, LearningProgressRepository>();
				builder.Services.AddScoped<ILearningProgressService, LearningProgressService>();
				builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
				builder.Services.AddScoped<IOutboxService, OutboxService>();

				// Add the infrastructure services, including the ServiceBusEnrollmentEventPublisher, which implements IEnrollmentEventPublisher.
				builder.Services.AddLearningPlatformInfrastructure(builder.Configuration);
			}

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.MapOpenApi();
			}

			app.UseHttpsRedirection();

			app.UseCors("ReactFrontEnd");

			app.UseDefaultFiles();					// For serving front end - static file support
			app.UseStaticFiles();                   // For serving front end - static file support

			app.UseAuthentication();
			app.UseAuthorization();

			app.MapControllers();

			app.MapFallbackToFile("index.html");    // For serving front end - SPA fallback

			app.Run();
		}

		private static IRecommendationAgentService CreateRecommendationAgentService(
			IServiceProvider serviceProvider,
			string foundryEndpoint,
			string foundryModel)
		{
			return new RecommendationAgentService(
				foundryEndpoint,
				foundryModel,
				serviceProvider.GetRequiredService<RecommendationTools>(),
				serviceProvider.GetRequiredService<ILogger<RecommendationAgentService>>());
		}

	}
}
