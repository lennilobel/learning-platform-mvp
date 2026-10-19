namespace LearningPlatform.DataLayer.Recommendations
{
	public sealed class LearnerContextResult
	{
		public long LearnerId { get; set; }

		public string TargetRole { get; set; } = string.Empty;

		public string CompletedCourseIds { get; set; } = "[]";

		public string Skills { get; set; } = "[]";
	}
}
