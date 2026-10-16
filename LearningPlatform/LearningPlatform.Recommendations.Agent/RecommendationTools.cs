using System.ComponentModel;
using System.Text.Json;
using LearningPlatform.Recommendations.Models;

namespace LearningPlatform.Recommendations.Agent
{
	public sealed class RecommendationTools
	{
		private readonly IRecommendationService _recommendationService;

		public RecommendationTools(IRecommendationService recommendationService)
		{
			this._recommendationService = recommendationService;
		}

		[Description("Gets ranked course recommendations for a learner. Only courses that satisfy deterministic eligibility rules are returned.")]
		public string GetRecommendations(
			[Description("The learner ID.")]
			int learnerId)
		{
			var learner = GetLearnerContext(learnerId);
			var courses = GetCourseCatalog();

			var recommendations = this._recommendationService.GetRecommendations(learner, courses);

			return JsonSerializer.Serialize(recommendations);
		}

		private static LearnerContext GetLearnerContext(int learnerId)
		{
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

		private static IReadOnlyList<Course> GetCourseCatalog()
		{
			return
			[
				new Course
				{
					CourseId = 100,
					Title = "C# Fundamentals",
					Role = "Azure Developer"
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
	}
}
