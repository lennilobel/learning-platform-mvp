namespace LearningPlatform.Recommendations.Agent
{
	namespace LearningPlatform.Recommendations.Agent
	{
		public interface IRecommendationAgentService
		{
			Task<string> RunAsync(string prompt);
		}
	}
}
