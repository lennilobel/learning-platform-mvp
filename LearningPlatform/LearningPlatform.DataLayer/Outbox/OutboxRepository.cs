using Microsoft.EntityFrameworkCore;

namespace LearningPlatform.DataLayer.Outbox
{
	public interface IOutboxRepository
	{
		Task<List<OutboxMessage>> GetUnpublishedOutboxMessages(int batchSize, CancellationToken cancellationToken = default);

		Task MarkOutboxMessagePublished(Guid outboxMessageId, DateTimeOffset publishedAt, CancellationToken cancellationToken = default);
	}

	public class OutboxRepository : IOutboxRepository
	{
		private readonly LearningPlatformDbContext _context;

		public OutboxRepository(LearningPlatformDbContext context)
		{
			this._context = context;
		}

		public async Task<List<OutboxMessage>> GetUnpublishedOutboxMessages(int batchSize, CancellationToken cancellationToken = default)
		{
			var results = await this._context
				.Set<OutboxMessage>()			// obtain the DbSet for the keyless entity type
				.FromSqlInterpolated(           // use FromSqlInterpolated to execute the stored procedure with parameters
					$"""
					EXEC dbo.GetUnpublishedOutboxMessages
						@BatchSize = {batchSize}
					""")
				.AsNoTracking()						// since we don't need EF to track the result
				.ToListAsync(cancellationToken);    // execute the query and get the results as a list, plus cancellation token to cancel the operation if needed

			return results;
		}

		public async Task MarkOutboxMessagePublished(
			Guid outboxMessageId,
			DateTimeOffset publishedAt,
			CancellationToken cancellationToken = default)
		{
			await this._context
				.Database						// access the underlying database
				.ExecuteSqlInterpolatedAsync(   // use ExecuteSqlInterpolatedAsync to execute the stored procedure with parameters
					$"""
					EXEC dbo.MarkOutboxMessagePublished
						@OutboxMessageId = {outboxMessageId},
						@PublishedAt = {publishedAt}
					""",
				cancellationToken);         // cancellation token to cancel the operation if needed
		}
	}

}
