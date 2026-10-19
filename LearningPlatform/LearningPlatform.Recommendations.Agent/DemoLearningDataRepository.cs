using LearningPlatform.Recommendations.Models;

namespace LearningPlatform.Recommendations.Agent
{
	public sealed class DemoLearningDataRepository : ILearningDataRepository
	{
		public Task<LearnerContext> GetLearnerContextAsync(long learnerId, CancellationToken cancellationToken = default)
		{
			if (learnerId != 1)
			{
				throw new KeyNotFoundException($"Learner {learnerId} was not found.");
			}

			var learnerContext = new LearnerContext
			{
				LearnerId = learnerId,
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

			return Task.FromResult(learnerContext);
		}

		// POC: Load the full catalog. A production implementation should query a reduced candidate set.
		public Task<IReadOnlyList<Course>> GetCourseCatalogAsync(CancellationToken cancellationToken = default)
		{
			IReadOnlyList<Course> courses =
			[
				new Course
				{
					CourseId = 100,
					Title = "C# Fundamentals",
					Role = "Azure Developer",
					Skills = new HashSet<string?>
					{
						"C#"
					}
				},
				new Course
				{
					CourseId = 200,
					Title = "Azure App Service",
					Role = "Azure Developer",
					PrerequisiteCourseIds = new HashSet<long>
					{
						100
					},
					Skills = new HashSet<string?>
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
					PrerequisiteCourseIds = new HashSet<long>
					{
						999
					},
					Skills = new HashSet<string?>
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
					Skills = new HashSet<string?>
					{
						"Power BI",
						"Data Visualization"
					}
				}
			];

			return Task.FromResult(courses);
		}

		public async Task<Course> GetCourseAsync(long courseId, CancellationToken cancellationToken = default)
		{
			var courses = await this.GetCourseCatalogAsync(cancellationToken);

			var course = courses.SingleOrDefault(course => course.CourseId == courseId);

			return course
				?? throw new KeyNotFoundException($"Course {courseId} was not found.");
		}
	}
}
