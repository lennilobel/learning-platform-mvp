using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace LearningPlatform.Recommendations.Agent.ConsoleUI
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			const string SourceName = "LearningPlatform.Recommendations.Agent";

			var builder = Host.CreateApplicationBuilder(args);

			var endpoint = builder.Configuration["Foundry:ProjectEndpoint"]
				?? throw new InvalidOperationException("Foundry:ProjectEndpoint is not configured.");

			var deploymentName = builder.Configuration["Foundry:Model"]
				?? throw new InvalidOperationException("Foundry:Model is not configured.");

			var eligibilityService = new EligibilityService();
			var courseRanker = new WeightedCourseRanker();

			var recommendationService = new RecommendationService(eligibilityService, courseRanker);

			var learningData = new DemoLearningData();

			var recommendationTools = new RecommendationTools(recommendationService, learningData);

			// Give us an ChatClientAgent for our composed agent pipeline.
			var baseAgent = RecommendationAgentFactory.Create(endpoint, deploymentName, recommendationTools);

			var agent =
				baseAgent
				.AsBuilder()                                // Gives us an AIAgentBuilder so we can build the agent pipeline.
				.UseOpenTelemetry(
					sourceName: SourceName,
					configure: options =>
						options.EnableSensitiveData = true) // Useful for controlled development; generally avoid in production.
				.Build();                                   // Gives us an AIAgent representing a composed agent pipeline
															// that wraps the original ChatClientAgent and adds telemetry.

			// Configure a tracer provider that listens for the agent's telemetry and exports completed activities to the console.
			using var tracerProvider =
				Sdk.CreateTracerProviderBuilder()
					.AddSource(SourceName)
					// If custom color-coding isn't needed, the standard console exporter could be used instead:
					//.AddConsoleExporter()
					.AddProcessor(new SimpleActivityExportProcessor(new ColoredConsoleActivityExporter(ConsoleColor.Cyan)))
					.Build();

			var response =
				await agent.RunAsync(
					"""
					For learner 1, recommend the best two courses.

					Also tell me:
					- the learner's target role,
					- the learner's existing skills,
					- the exact prerequisite course IDs for each recommendation,
					- and all skills developed by each recommended course.

					Explain why the first recommendation ranks above the second.
					""");

			Console.WriteLine(response);
		}
	}
}
