using LearningPlatform.Shared;
using Microsoft.EntityFrameworkCore;

namespace LearningPlatform.DataLayer.LearningProgress
{
	public interface ILearningProgressRepository
	{
		Task<bool> ProcessEnrollmentCreated(EnrollmentCreatedEvent enrollmentCreatedEvent, CancellationToken cancellationToken = default);
	}

	public class LearningProgressRepository : ILearningProgressRepository
	{
		private readonly LearningPlatformDbContext _context;

		public LearningProgressRepository(LearningPlatformDbContext context)
		{
			this._context = context;
		}

		public async Task<bool> ProcessEnrollmentCreated(EnrollmentCreatedEvent enrollmentCreatedEvent, CancellationToken cancellationToken = default)
		{
			var results = await this._context
				.Set<ProcessEnrollmentCreatedResult>()
				.FromSqlInterpolated(
					$"""
					EXEC dbo.ProcessEnrollmentCreated
						@MessageId = {enrollmentCreatedEvent.EventId},
						@LearnerId = {enrollmentCreatedEvent.LearnerId},
						@CourseId = {enrollmentCreatedEvent.CourseId},
						@OccurredAt = {enrollmentCreatedEvent.OccurredAt}
					""")
				.AsNoTracking()
				.ToListAsync(cancellationToken);

			var result = results.Single();

			var isDuplicate = result.IsDuplicate;

			return isDuplicate;
		}
	}
}
