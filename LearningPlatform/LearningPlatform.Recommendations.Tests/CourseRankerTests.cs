using LearningPlatform.Recommendations.Models;

namespace LearningPlatform.Recommendations.Tests
{
	public class CourseRankerTests
	{
		[Fact]
		public void RankCourses_AppliesWeightedRanking()
		{
			var learner = new LearnerContext
			{
				LearnerId = 1,
				TargetRole = "Azure Developer",
				Skills = new HashSet<string>
				{
					"C#",
					"ASP.NET Core"
				}
			};

			var eligibleCourses = new[]
			{
				// Matches on role and two new skills (should score 50 + 10 + 10 = 70)
				new EligibleCourse
				{
					Course = new Course
					{
						CourseId = 200,
						Title = "Azure App Service",
						Role = "Azure Developer",
						Skills = new HashSet<string>
						{
							"Azure",
							"App Service"
						}
					},
					Reasons = []
				},
				// Doesn't match on role, but does match on two new skills (should score 10 + 10 = 20)
				new EligibleCourse
				{
					Course = new Course
					{
						CourseId = 400,
						Title = "Power BI Fundamentals",
						Role = "Data Analyst",
						Skills = new HashSet<string>
						{
							"Power BI",
							"Data Visualization"
						}
					},
					Reasons = []
				}
			};

			var ranker = new WeightedCourseRanker();

			var results = ranker.RankCourses(learner, eligibleCourses);

			// There should be two eligible courses in the results
			Assert.Equal(2, results.Count);

			// The first one should be course ID 200 with a score of 70
			Assert.Equal(200, results[0].Course.CourseId);
			Assert.Equal(70, results[0].Score);

			// The second one should be course ID 400 with a score of 20
			Assert.Equal(400, results[1].Course.CourseId);
			Assert.Equal(20, results[1].Score);

		}
	}
}
