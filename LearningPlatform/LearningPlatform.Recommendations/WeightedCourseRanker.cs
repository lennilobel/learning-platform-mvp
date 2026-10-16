using LearningPlatform.Recommendations.Models;

namespace LearningPlatform.Recommendations
{
	public sealed class WeightedCourseRanker : ICourseRanker
	{
		public IReadOnlyList<RankedCourse> RankCourses(LearnerContext learner, IEnumerable<EligibleCourse> eligibleCourses)
		{
			var results = new List<RankedCourse>();

			foreach (var eligibleCourse in eligibleCourses)
			{
				var course = eligibleCourse.Course;
				var score = 0;
				var reasons = new List<string>(eligibleCourse.Reasons);

				// Add weight for matching role between learner and eligible course
				if (string.Equals(course.Role, learner.TargetRole, StringComparison.OrdinalIgnoreCase))
				{
					score += 50;
					reasons.Add($"Course matches target role '{learner.TargetRole}'.");
				}

				// Add weight for skills taught by the course that the learner does not already have
				foreach (var skill in course.Skills)
				{
					if (!learner.Skills.Contains(skill))
					{
						score += 10;
						reasons.Add($"Course develops new skill '{skill}'.");
					}
				}

				results.Add(new RankedCourse
				{
					Course = course,
					Score = score,
					Reasons = reasons
				});
			}

			return results
				.OrderByDescending(result => result.Score)
				.ThenBy(result => result.Course.CourseId)
				.ToList();
		}
	}
}
