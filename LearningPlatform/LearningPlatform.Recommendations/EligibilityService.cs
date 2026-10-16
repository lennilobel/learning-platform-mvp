using LearningPlatform.Recommendations.Models;

namespace LearningPlatform.Recommendations
{
	public sealed class EligibilityService : IEligibilityService
	{
		public IReadOnlyList<EligibleCourse> GetEligibleCourses(LearnerContext learner, IEnumerable<Course> courses)
		{
			var results = new List<EligibleCourse>();

			foreach (var course in courses)
			{
				// The course must be active
				if (!course.IsActive)
				{
					continue;
				}

				// It must not be a course already completed by the learner
				if (learner.CompletedCourseIds.Contains(course.CourseId))
				{
					continue;
				}

				// The learner must have completed all of the course's prerequisite courses
				if (!course.PrerequisiteCourseIds.All(prerequisiteCourseId => learner.CompletedCourseIds.Contains(prerequisiteCourseId)))
				{
					continue;
				}

				// Note that we do *not* make a course ineligible if it does not match the learner's role. It is
				// still eligible, even though it may not be a good recommendation. Relevance is the responsibility
				// of the ranking layer, not deterministic eligibility policy.

				results.Add(new EligibleCourse
				{
					Course = course,
					Reasons =
					[
						"Course is active.",
						"Course has not already been completed.",
						"All prerequisites are satisfied."
					]
				});
			}

			return results;
		}
	}
}
