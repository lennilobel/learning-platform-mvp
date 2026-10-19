namespace LearningPlatform.Recommendations.Models
{
	public sealed class LearnerContext
	{
		// The learner ID
		public required long LearnerId { get; init; }

		// The learner's target role
		public required string TargetRole { get; init; }

		// Courses completed by the learner
		public IReadOnlySet<long> CompletedCourseIds { get; init; } = new HashSet<long>();

		// Skills possessed by the learner
		public IReadOnlySet<string> Skills { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	}
}
