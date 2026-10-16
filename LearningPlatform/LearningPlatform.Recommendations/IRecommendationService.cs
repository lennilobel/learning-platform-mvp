using LearningPlatform.Recommendations.Models;

namespace LearningPlatform.Recommendations
{
	public interface IRecommendationService
	{
		IReadOnlyList<RankedCourse> GetRecommendations(LearnerContext learner, IEnumerable<Course> courses);
	}
}
