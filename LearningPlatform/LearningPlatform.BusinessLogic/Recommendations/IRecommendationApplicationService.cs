using LearningPlatform.Recommendations.Models;

namespace LearningPlatform.BusinessLogic.Recommendations
{
	public interface IRecommendationApplicationService
	{
		Task<IReadOnlyList<RankedCourse>> GetRecommendationsAsync(long learnerId, int maxResults, CancellationToken cancellationToken = default);
	}
}
