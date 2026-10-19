using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace LearningPlatform.Recommendations.Agent
{
	public static class RecommendationAgentFactory
	{
		public static ChatClientAgent Create(
			string endpoint,
			string deploymentName,
			RecommendationTools recommendationTools,
			EnrollmentTools? enrollmentTools = null)    // Optionally accept the enrollment tools. This lets the evaluation agent remain read-only while the actual console gets HITL enrollment.
		{
			var tools = new List<AITool>
			{
				AIFunctionFactory.Create(recommendationTools.GetLearnerProfile),
				AIFunctionFactory.Create(recommendationTools.GetRecommendations),
				AIFunctionFactory.Create(recommendationTools.GetCourseDetails)
			};

			if (enrollmentTools is not null)
			{
				// HITL (human-in-the-loop)
				tools.Add(new ApprovalRequiredAIFunction(AIFunctionFactory.Create(enrollmentTools.EnrollLearner)));
			}

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
					tools: tools)
			;

			// Return the ChatClientAgent, which then acts as the underlying agent for a composed an agent pipeline (e.g., to include OpenTelemetry).
			return agent;
		}
	}
}
