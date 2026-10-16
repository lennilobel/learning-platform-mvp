namespace LearningPlatform.Shared
{
	public class EnrollmentCreatedEvent
	{
		public Guid EventId { get; set; }
		public long LearnerId { get; set; }
		public long CourseId { get; set; }
		public DateTimeOffset OccurredAt { get; set; }
	}
}
