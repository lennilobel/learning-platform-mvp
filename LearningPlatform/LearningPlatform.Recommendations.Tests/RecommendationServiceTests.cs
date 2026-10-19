using LearningPlatform.Recommendations.Models;

namespace LearningPlatform.Recommendations.Tests
{
	public class RecommendationServiceTests
	{
		[Fact]
		public void GetRecommendations_FiltersThenRanksCourses()
		{
			var learner = new LearnerContext
			{
				LearnerId = 1,
				TargetRole = "Azure Developer",
				CompletedCourseIds = new HashSet<long>
				{
					100
				},
				Skills = new HashSet<string>
				{
					"C#",
					"ASP.NET Core"
				}
			};

			var courses = new[]
			{
				// Should be ineligible because the learner already completed this course.
				new Course
				{
					CourseId = 100,
					Title = "C# Fundamentals",
					Role = "Azure Developer"
				},
				// Should be eligible because it's active, hasn't been completed yet, and satisfies the prerequisite courses.
				// Matches on role and two new skills (should score 50 + 10 + 10 = 70)
				new Course
				{
					CourseId = 200,
					Title = "Azure App Service",
					Role = "Azure Developer",
					PrerequisiteCourseIds = new HashSet<long>
					{
						100
					},
					Skills = new HashSet<string>
					{
						"Azure",
						"App Service"
					}
				},
				// Should be ineligible because the learner does not satisfy the prerequisite courses.
				new Course
				{
					CourseId = 300,
					Title = "Advanced AKS",
					Role = "Azure Developer",
					PrerequisiteCourseIds = new HashSet<long>
					{
						999
					},
					Skills = new HashSet<string>
					{
						"AKS",
						"Kubernetes"
					}
				},
				// Doesn't match on role, but does match on two new skills (should score 10 + 10 = 20)
				// Should be eligible because it's active, hasn't been completed yet, and satisfies the prerequisite courses.
				new Course
				{
					CourseId = 400,
					Title = "Power BI Fundamentals",
					Role = "Data Analyst",
					Skills = new HashSet<string>
					{
						"Power BI",
						"Data Visualization"
					}
				}
			};

			var eligibilityService = new EligibilityService();
			var courseRanker = new WeightedCourseRanker();

			var recommendationService = new RecommendationService(eligibilityService, courseRanker);

			var results = recommendationService.GetRecommendations(learner, courses);

			// There should be two courses in the results with course IDs 200 and 400
			Assert.Equal(
				[200, 400],
				results.Select(result => result.Course.CourseId));

			// They should respectively be scored 70 and 20
			Assert.Equal(
				[70, 20],
				results.Select(result => result.Score));
		}
	}
}
