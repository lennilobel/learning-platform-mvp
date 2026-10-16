namespace LearningPlatform.Recommendations.Models
{
	public sealed class LearnerContext
	{
		// The learner ID
		public required int LearnerId { get; init; }

		// The learner's target role
		public required string TargetRole { get; init; }

		// Courses completed by the learner
		public IReadOnlySet<int> CompletedCourseIds { get; init; } = new HashSet<int>();

		// Skills possessed by the learner
		public IReadOnlySet<string> Skills { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	}
}
