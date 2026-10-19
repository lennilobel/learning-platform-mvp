using LearningPlatform.Recommendations.Agent.LearningPlatform.Recommendations.Agent;

namespace LearningPlatform.Recommendations.Agent
{
	public sealed class RecommendationAgentService : IRecommendationAgentService
	{
		private readonly string _endpoint;
		private readonly string _deploymentName;
		private readonly RecommendationTools _recommendationTools;

		public RecommendationAgentService(
			string endpoint,
			string deploymentName,
			RecommendationTools recommendationTools)
		{
			this._endpoint = endpoint;
			this._deploymentName = deploymentName;
			this._recommendationTools = recommendationTools;
		}

		public async Task<string> RunAsync(string prompt)
		{
			// Create a read-only agent. EnrollmentTools is intentionally omitted,
			// so this stateless HTTP endpoint cannot perform side-effecting enrollment.
			var agent =
				RecommendationAgentFactory.Create(
					this._endpoint,
					this._deploymentName,
					this._recommendationTools);

			var response = await agent.RunAsync(prompt);

			return response.ToString();
		}
	}
}
