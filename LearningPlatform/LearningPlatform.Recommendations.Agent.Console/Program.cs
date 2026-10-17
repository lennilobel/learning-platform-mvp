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

			var learningData = new DemoLearningData();

			var recommendationTools = new RecommendationTools(recommendationService, learningData);

			var agent =
				new AIProjectClient(
					new Uri(endpoint),
					new DefaultAzureCredential())
				.AsAIAgent(
					model: deploymentName,
					name:
						"LearningRecommendationAgent",
					instructions:
						"""
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
					]);

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

			System.Console.WriteLine(response);
		}
	}
}
