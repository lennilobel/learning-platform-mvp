namespace LearningPlatform.DataLayer.Recommendations
{
	public sealed class CourseResult
	{
		public long CourseId { get; set; }

		public string Title { get; set; } = string.Empty;

		public string Role { get; set; } = string.Empty;

		public bool IsActive { get; set; }

		public string PrerequisiteCourseIds { get; set; } = "[]";

		public string Skills { get; set; } = "[]";
	}
}
