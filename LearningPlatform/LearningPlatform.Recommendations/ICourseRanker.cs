using LearningPlatform.Recommendations.Models;

namespace LearningPlatform.Recommendations
{
	public interface ICourseRanker
	{
		IReadOnlyList<RankedCourse> RankCourses(LearnerContext learner, IEnumerable<EligibleCourse> eligibleCourses);
	}
}
