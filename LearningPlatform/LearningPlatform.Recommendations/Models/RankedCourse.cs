namespace LearningPlatform.Recommendations.Models
{
	public sealed class RankedCourse
	{
		public required Course Course { get; init; }

		public required int Score { get; init; }

		public required IReadOnlyList<string> Reasons { get; init; }
	}
}
