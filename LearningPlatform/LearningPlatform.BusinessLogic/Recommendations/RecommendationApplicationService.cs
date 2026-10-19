using LearningPlatform.Recommendations;
using LearningPlatform.Recommendations.Models;

namespace LearningPlatform.BusinessLogic.Recommendations
{
	public sealed class RecommendationApplicationService : IRecommendationApplicationService
	{
		private readonly IRecommendationService _recommendationService;
		private readonly ILearningDataRepository _learningDataRepository;

		public RecommendationApplicationService(
			IRecommendationService recommendationService,
			ILearningDataRepository learningDataRepository)
		{
			this._recommendationService = recommendationService;
			this._learningDataRepository = learningDataRepository;
		}

		public async Task<IReadOnlyList<RankedCourse>> GetRecommendationsAsync(long learnerId, int maxResults, CancellationToken cancellationToken = default)
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxResults);

			var learnerContext = await this._learningDataRepository.GetLearnerContextAsync(learnerId, cancellationToken);

			// POC: Load the full catalog. A production implementation should query a reduced candidate set.
			var courses = await this._learningDataRepository.GetCourseCatalogAsync(cancellationToken);

			var recommendations = this._recommendationService.GetRecommendations(learnerContext, courses)
				.Take(maxResults)
				.ToList();

			return recommendations;
		}
	}
}
