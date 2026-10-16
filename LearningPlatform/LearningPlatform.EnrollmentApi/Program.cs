using LearningPlatform.BusinessLogic.Enrollments;
using LearningPlatform.BusinessLogic.LearningProgress;
using LearningPlatform.BusinessLogic.Outbox;
using LearningPlatform.DataLayer;
using LearningPlatform.DataLayer.Enrollment;
using LearningPlatform.DataLayer.LearningProgress;
using LearningPlatform.DataLayer.Outbox;
using LearningPlatform.Infrastructure;
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

			// Configure the database context to use SQL Server with the connection string from appsettings.json
			// Note that this uses scoped lifetime, because the DbContext is not thread-safe and should not be shared across requests.
			builder.Services.AddDbContext<LearningPlatformDbContext>(options =>
				options.UseSqlServer(
					builder.Configuration.GetSection("ConnectionStrings")["LearningPlatform"]));

			// Even though our services and repositories are stateless (and could therefore be registered as singletons), we can use
			// scoped lifetime for them. This is because they depend on other services that are scoped; namely, the DbContext.
			builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
			builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

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
	}
}
