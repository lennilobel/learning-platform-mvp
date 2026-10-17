using System.ComponentModel;
using System.Text.Json;

namespace LearningPlatform.Recommendations.Agent
{
	public sealed class RecommendationTools
	{
		private readonly IRecommendationService _recommendationService;
		private readonly DemoLearningData _learningData;

		public RecommendationTools(
			IRecommendationService recommendationService,
			DemoLearningData learningData)
		{
			this._recommendationService = recommendationService;
			this._learningData = learningData;
		}

		// Note the use of the [Description] attribute. Traditionally, the [Description] attribute
		// provided descriptive metadata for reflection-based tooling and component frameworks (e.g.,
		// property browsers and designers). Nowadays, Agent Framework uses it as well for constructing
		// the tool metadata presented to the LLM.
		//
		// Without [Description] attributes, the model still has method names, parameter names, and
		// parameter types, so good naming may allow it to infer the intended semantics anyway. But
		// the [Description] attributes make that contract more explicit and can express domain semantics
		// that the type system alone cannot convey.

		[Description("Gets a learner's profile, including target role, completed courses, and current skills.")]
		public string GetLearnerProfile(
			[Description("The learner ID.")] int learnerId)
		{
			var learnerContext = this._learningData.GetLearnerContext(learnerId);

			// Return the profile based on the complete learner context.
			var learnerProfileJson = JsonSerializer.Serialize(learnerContext);

			return learnerProfileJson;
		}

		[Description("Gets the highest-ranked eligible course recommendations for a learner.")]
		public string GetRecommendations(
			[Description("The learner ID.")] int learnerId,
			[Description("The maximum number of recommendations to return.")] int maxResults)
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxResults);

			var learnerContext = this._learningData.GetLearnerContext(learnerId);

			var courses = this._learningData.GetCourseCatalog();

 			var recommendations = this._recommendationService
				.GetRecommendations(learnerContext, courses)
				.Take(maxResults);

			// Return only recommendation summary data here. Detailed course metadata is intentionally owned by the GetCourseDetails tool,
			// keeping the tool contracts focused and allowing the agent to retrieve details only when needed.
			var recommendationsJson =
				JsonSerializer.Serialize(
					recommendations.Select(
						recommendation => new
						{
							recommendation.Course.CourseId,
							recommendation.Course.Title,
							recommendation.Score,
							recommendation.Reasons
						}));

			return recommendationsJson;
		}

		[Description("Gets detailed information about a specific course, including its role, skills, active status, and prerequisites.")]
		public string GetCourseDetails(
			[Description("The course ID.")]
			int courseId)
		{
			var course = this._learningData.GetCourse(courseId);

			// Return the course details based on the complete course entity.
			var courseDetailsJson = JsonSerializer.Serialize(course);

			return courseDetailsJson;
		}
	}
}
