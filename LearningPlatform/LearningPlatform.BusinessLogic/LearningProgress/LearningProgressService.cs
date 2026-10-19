using LearningPlatform.DataLayer.LearningProgress;
using LearningPlatform.Shared;
using Microsoft.Extensions.Logging;

namespace LearningPlatform.BusinessLogic.LearningProgress
{
	public interface ILearningProgressService
	{
		Task<bool> ProcessEnrollmentCreated(EnrollmentCreatedEvent enrollmentCreatedEvent, CancellationToken cancellationToken = default);
	}

	public class LearningProgressService : ILearningProgressService
	{
		private readonly ILogger<LearningProgressService> _logger;
		private readonly ILearningProgressRepository _learningProgressRepository;

		public LearningProgressService(
			ILogger<LearningProgressService> logger,
			ILearningProgressRepository learningProgressRepository)
		{
			this._logger = logger;
			this._learningProgressRepository = learningProgressRepository;
		}

		public async Task<bool> ProcessEnrollmentCreated(
			EnrollmentCreatedEvent enrollmentCreatedEvent,
			CancellationToken cancellationToken = default)
		{
			// Duplicate check is required for idempotency, because the Service Bus subscription may redeliver the same message if the consumer fails to complete it (e.g., if the consumer crashes or throws an exception before calling CompleteMessageAsync)
			var isDuplicate = await this._learningProgressRepository.ProcessEnrollmentCreated(enrollmentCreatedEvent, cancellationToken);

			this._logger.LogInformation("Processed EnrollmentCreated business logic. EventId={EventId}, LearnerId={LearnerId}, CourseId={CourseId}, IsDuplicate={IsDuplicate}",
				enrollmentCreatedEvent.EventId,
				enrollmentCreatedEvent.LearnerId,
				enrollmentCreatedEvent.CourseId,
				isDuplicate);

			return isDuplicate;
		}
	}

}
