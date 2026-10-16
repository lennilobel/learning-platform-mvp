using LearningPlatform.Shared;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LearningPlatform.DataLayer.Enrollment
{
	public interface IEnrollmentRepository
	{
		Task<bool> CreateEnrollment(EnrollmentCreatedEvent enrollmentCreatedEvent, CancellationToken cancellationToken = default);
	}

	public class EnrollmentRepository : IEnrollmentRepository
	{
		private static readonly JsonSerializerOptions _jsonSerializerOptions =
			new(JsonSerializerDefaults.Web); 
		
		private readonly LearningPlatformDbContext _context;

		public EnrollmentRepository(LearningPlatformDbContext context)
		{
			this._context = context;
		}

		public async Task<bool> CreateEnrollment(EnrollmentCreatedEvent enrollmentCreatedEvent, CancellationToken cancellationToken = default)
		{
			var messageType = nameof(EnrollmentCreatedEvent);

			var payload = JsonSerializer.Serialize(enrollmentCreatedEvent, _jsonSerializerOptions);

			var results = await this._context
				.Set<CreateEnrollmentResult>()  // obtain the DbSet for the keyless entity type
				.FromSqlInterpolated(           // use FromSqlInterpolated to execute the stored procedure with parameters
					$"""
					EXEC dbo.CreateEnrollment
						@LearnerId = {enrollmentCreatedEvent.LearnerId},
						@CourseId = {enrollmentCreatedEvent.CourseId},
						@EventId = {enrollmentCreatedEvent.EventId},
						@MessageType = {messageType},
						@Payload = {payload},
						@OccurredAt = {enrollmentCreatedEvent.OccurredAt}
					""")
				.AsNoTracking()     // since we don't need EF to track the result
				.ToListAsync(cancellationToken);

			var result = results.Single();

			var isDuplicate = result.IsDuplicate;

			return isDuplicate;
		}
	}
}
