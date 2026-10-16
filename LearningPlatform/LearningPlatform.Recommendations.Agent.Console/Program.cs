using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;

namespace LearningPlatform.Recommendations.Agent.Console
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var builder = Host.CreateApplicationBuilder(args);

			var endpoint = builder.Configuration["Foundry:ProjectEndpoint"]
				?? throw new InvalidOperationException("Foundry:ProjectEndpoint is not configured.");

			var deploymentName = builder.Configuration["Foundry:Model"]
				?? throw new InvalidOperationException("Foundry:Model is not configured.");

			var eligibilityService = new EligibilityService();
			var courseRanker = new WeightedCourseRanker();

			var recommendationService = new RecommendationService(eligibilityService, courseRanker);

			var recommendationTools = new RecommendationTools(recommendationService);

			var agent =
				new AIProjectClient(
					new Uri(endpoint),
					new DefaultAzureCredential())
				.AsAIAgent(
					model: deploymentName,
					name: "LearningRecommendationAgent",
					instructions:
						"""
						You are a learning recommendation assistant.

						When asked what a learner should study next,
						you must use the GetRecommendations tool.

						You may explain and summarize the returned recommendations,
						but you must not recommend courses that are not returned
						by the tool.
						""",
					tools:
					[
						AIFunctionFactory.Create(recommendationTools.GetRecommendations)
					]);

			System.Console.WriteLine(
				await agent.RunAsync("What should learner 1 study next, and why?"));
		}
	}
}
