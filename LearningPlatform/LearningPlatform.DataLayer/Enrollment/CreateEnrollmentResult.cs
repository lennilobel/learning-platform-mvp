using System.ComponentModel.DataAnnotations.Schema;

namespace LearningPlatform.DataLayer.Enrollment
{
	public class CreateEnrollmentResult
	{
		public bool IsDuplicate { get; set; }
		public EnrollmentValidationErrors ValidationErrors { get; set; } = new EnrollmentValidationErrors();
	}

	[NotMapped]
	public class EnrollmentValidationErrors
	{
		public string? LearnerId { get; set; }
		public string? CourseId { get; set; }
	}
}
