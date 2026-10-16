namespace LearningPlatform.Shared
{
	public class Enrollment
	{
		public long LearnerId { get; set; }
		public long CourseId { get; set; }

		public override bool Equals(object? obj) =>
			obj is Enrollment other &&
				this.LearnerId == other.LearnerId &&
				this.CourseId == other.CourseId;

		public override int GetHashCode() =>
			HashCode.Combine(this.LearnerId, this.CourseId);

	}
}
