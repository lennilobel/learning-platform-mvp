using LearningPlatform.Recommendations.Models;

namespace LearningPlatform.Recommendations.Tests
{
	public class EligibilityServiceTests
	{
		[Fact]
		public void GetEligibleCourses_AppliesDeterministicConstraints()
		{
			var learner = new LearnerContext
			{
				LearnerId = 1,
				TargetRole = "Azure Developer",
				CompletedCourseIds = new HashSet<int>
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
				new Course
				{
					CourseId = 200,
					Title = "Azure App Service",
					Role = "Azure Developer",
					PrerequisiteCourseIds = new HashSet<int>
					{
						100
					}
				},
				// Should be ineligible because the learner does not satisfy the prerequisite courses.
				new Course
				{
					CourseId = 300,
					Title = "Advanced AKS",
					Role = "Azure Developer",
					PrerequisiteCourseIds = new HashSet<int>
					{
						999
					}
				},
				// Should be eligible because it's active, hasn't been completed yet, and satisfies the prerequisite courses.
				new Course
				{
					CourseId = 400,
					Title = "Power BI Fundamentals",
					Role = "Data Analyst"
				},
				// Should be ineligible because it's not active.
				new Course
				{
					CourseId = 500,
					Title = "Retired Azure Course",
					Role = "Azure Developer",
					IsActive = false
				}
			};

			var service = new EligibilityService();

			var results = service.GetEligibleCourses(learner, courses);

			// There should be two eligible courses in the results
			Assert.Equal(2, results.Count);

			// One of them should be course ID 200
			Assert.Contains(results, result => result.Course.CourseId == 200);

			// The other one should be course ID 400
			Assert.Contains(results, result => result.Course.CourseId == 400);
		}
	}
}
