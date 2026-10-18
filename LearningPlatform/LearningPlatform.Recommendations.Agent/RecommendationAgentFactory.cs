using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace LearningPlatform.Recommendations.Agent
{
	public static class RecommendationAgentFactory
	{
		public static ChatClientAgent Create(string endpoint, string deploymentName, RecommendationTools recommendationTools)
		{
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
			;

			// Return the ChatClientAgent, which then acts as the underlying agent for a composed an agent pipeline (e.g., to include OpenTelemetry).
			return agent;
		}
	}
}
