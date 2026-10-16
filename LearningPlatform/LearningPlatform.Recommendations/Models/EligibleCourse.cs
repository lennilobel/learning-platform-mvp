namespace LearningPlatform.Recommendations.Models
{
	public sealed class EligibleCourse
	{
		public required Course Course { get; init; }

		public required IReadOnlyList<string> Reasons { get; init; }
	}
}
