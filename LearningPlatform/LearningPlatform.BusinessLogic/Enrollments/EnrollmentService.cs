using LearningPlatform.DataLayer.Enrollment;
using LearningPlatform.Shared;
using Microsoft.Extensions.Logging;

namespace LearningPlatform.BusinessLogic.Enrollments
{
	public interface IEnrollmentService
	{
		Task<CreateEnrollmentResult> CreateEnrollment(Enrollment enrollment, CancellationToken cancellationToken);
	}

	public class EnrollmentService : IEnrollmentService
	{
		private readonly ILogger<EnrollmentService> _logger;
		private readonly IEnrollmentRepository _enrollmentRepository;

		public EnrollmentService(
			ILogger<EnrollmentService> logger,
			IEnrollmentRepository enrollmentRepository)
		{
			this._logger = logger;
			this._enrollmentRepository = enrollmentRepository;
		}

		public async Task<CreateEnrollmentResult> CreateEnrollment(Enrollment enrollment, CancellationToken cancellationToken)
		{
			this._logger.LogInformation("EnrollmentService creating enrollment. LearnerId={LearnerId}, CourseId={CourseId}",
				enrollment.LearnerId,
				enrollment.CourseId);

			// Mock server-side validation
			var validationErrors = new EnrollmentValidationErrors
			{
				LearnerId = enrollment.LearnerId == 666 ? "This learner may only be requested by the devil!" : null,
				CourseId = enrollment.CourseId == 666 ? "This course may only be requested by the devil!" : null
			};
			if (validationErrors.LearnerId != null || validationErrors.CourseId != null)
			{
				return new()
				{
					ValidationErrors = validationErrors,
				};
			}

			// Server-side validation passed; create the enrollment
			var enrollmentCreatedEvent = new EnrollmentCreatedEvent
			{
				EventId = Guid.NewGuid(),
				LearnerId = enrollment.LearnerId,
				CourseId = enrollment.CourseId,
				OccurredAt = DateTimeOffset.UtcNow,
			};

			var isDuplicate = await this._enrollmentRepository.CreateEnrollment(enrollmentCreatedEvent, cancellationToken);

			this._logger.LogInformation("EnrollmentService created enrollment. LearnerId={LearnerId}, CourseId={CourseId}, IsDuplicate={IsDuplicate}",
				enrollment.LearnerId,
				enrollment.CourseId,
				isDuplicate);

			return new()
			{
				IsDuplicate = isDuplicate
			};
		}
	}
}
