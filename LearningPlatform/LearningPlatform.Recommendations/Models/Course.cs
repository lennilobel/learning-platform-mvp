namespace LearningPlatform.Recommendations.Models
{
	public sealed class Course
	{
		// The course ID
		public required int CourseId { get; init; }

		// The course title
		public required string Title { get; init; }

		// The course role
		public required string Role { get; init; }

		// The course's active flag
		public bool IsActive { get; init; } = true;

		// Course IDs of all prerequisite courses
		public IReadOnlySet<int> PrerequisiteCourseIds { get; init; } = new HashSet<int>();

		// New skills that the course develops
		public IReadOnlySet<string> Skills { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	}
}
