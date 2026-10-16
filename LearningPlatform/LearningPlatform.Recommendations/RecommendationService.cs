using LearningPlatform.Recommendations.Models;

namespace LearningPlatform.Recommendations
{
	public sealed class RecommendationService : IRecommendationService
	{
		private readonly IEligibilityService _eligibilityService;
		private readonly ICourseRanker _courseRanker;

		public RecommendationService(IEligibilityService eligibilityService, ICourseRanker courseRanker)
		{
			this._eligibilityService = eligibilityService;
			this._courseRanker = courseRanker;
		}

		public IReadOnlyList<RankedCourse> GetRecommendations(LearnerContext learner, IEnumerable<Course> courses)
		{
			// Filter the course catalog to only those courses that the learner is eligible for
			var eligibleCourses = this._eligibilityService.GetEligibleCourses(learner, courses);

			// Rank the eligible courses for the learner
			var rankedCourses = this._courseRanker.RankCourses(learner, eligibleCourses);

			return rankedCourses;
		}
	}
}
