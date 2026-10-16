using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using LearningPlatform.BusinessLogic.LearningProgress;
using LearningPlatform.DataLayer;
using LearningPlatform.DataLayer.LearningProgress;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Logging.AddOpenTelemetry(logging =>
{
	logging.IncludeFormattedMessage = true;
	logging.IncludeScopes = true;
}); 

var openTelemetry = builder.Services
	.AddOpenTelemetry()
	.UseFunctionsWorkerDefaults();

// When running in Azure, the Application Insights connection string will be configured to enable the Azure Monitor exporter for OpenTelemetry.
var applicationInsightsConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
{
	var managedIdentityClientId = builder.Configuration["AzureMonitor:ManagedIdentityClientId"]
		?? throw new InvalidOperationException("Azure Monitor managed identity client ID is not configured.");

	openTelemetry.UseAzureMonitorExporter(options =>
	{
		options.ConnectionString = applicationInsightsConnectionString;
		options.Credential = new ManagedIdentityCredential(ManagedIdentityId.FromUserAssignedClientId(managedIdentityClientId));
	});
}

// Configure the database context to use SQL Server with the connection string from configuration.
builder.Services.AddDbContext<LearningPlatformDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetSection("ConnectionStrings")["LearningPlatform"]));

// Register the learning progress repository and business service used by the Function.
builder.Services.AddScoped<ILearningProgressRepository, LearningProgressRepository>();
builder.Services.AddScoped<ILearningProgressService, LearningProgressService>();

builder.Build().Run();
