using LearningPlatform.Recommendations.Models;

namespace LearningPlatform.Recommendations
{
	public interface IEligibilityService
	{
		IReadOnlyList<EligibleCourse> GetEligibleCourses(LearnerContext learner, IEnumerable<Course> courses);
	}
}
