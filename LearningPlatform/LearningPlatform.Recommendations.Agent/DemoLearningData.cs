using LearningPlatform.Recommendations.Models;

namespace LearningPlatform.Recommendations.Agent
{
	public sealed class DemoLearningData
	{
		public LearnerContext GetLearnerContext(int learnerId)
		{
			if (learnerId != 1)
			{
				throw new KeyNotFoundException($"Learner {learnerId} was not found.");
			}

			return new LearnerContext
			{
				LearnerId = learnerId,
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
		}

		public IReadOnlyList<Course> GetCourseCatalog()
		{
			return
			[
				new Course
				{
					CourseId = 100,
					Title = "C# Fundamentals",
					Role = "Azure Developer",
					Skills = new HashSet<string>
					{
						"C#"
					}
				},
				new Course
				{
					CourseId = 200,
					Title = "Azure App Service",
					Role = "Azure Developer",
					PrerequisiteCourseIds = new HashSet<int>
					{
						100
					},
					Skills = new HashSet<string>
					{
						"Azure",
						"App Service"
					}
				},
				new Course
				{
					CourseId = 300,
					Title = "Advanced AKS",
					Role = "Azure Developer",
					PrerequisiteCourseIds = new HashSet<int>
					{
						999
					},
					Skills = new HashSet<string>
					{
						"AKS",
						"Kubernetes"
					}
				},
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
			];
		}

		public Course GetCourse(int courseId)
		{
			var course = this.GetCourseCatalog()
				.SingleOrDefault(course => course.CourseId == courseId);

			return course
				?? throw new KeyNotFoundException($"Course {courseId} was not found.");
		}
	}
}
