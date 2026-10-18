using Azure.AI.Projects;
using Azure.Identity;
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

			// Note: concrete class ChatClientAgent derives from abstract base class AIAgent.
			// AIAgentBuilder can then compose an AIAgent pipeline around that underlying agent.
			var agent =
				new AIProjectClient(                        // Gives us an AIProjectClient.
					new Uri(endpoint),
					new DefaultAzureCredential())
				.AsAIAgent(                                 // Gives us a concrete ChatClientAgent (which derives from the abstract AIAgent).
					model: deploymentName,
					name: "LearningRecommendationAgent",
					instructions: """
						You are a learning recommendation assistant.

						Use the available tools whenever learner-specific
						or course-specific facts are needed.

						Never invent learner or course information.

						All course recommendations must come from the
						GetRecommendations tool.

						Do not treat a course as recommended merely because
						GetCourseDetails can retrieve information about it.

						Use GetLearnerProfile when learner profile information
						is needed.

						Use GetCourseDetails when detailed information about
						a course is needed.
						""",
					tools:
					[
						AIFunctionFactory.Create(recommendationTools.GetLearnerProfile),
						AIFunctionFactory.Create(recommendationTools.GetRecommendations),
						AIFunctionFactory.Create(recommendationTools.GetCourseDetails)
					])
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
