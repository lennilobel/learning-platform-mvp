using LearningPlatform.DataLayer.Outbox;
using Microsoft.Extensions.Logging;

namespace LearningPlatform.BusinessLogic.Outbox
{
	public interface IOutboxService
	{
		Task ProcessOutbox(CancellationToken cancellationToken);
	}

	public class OutboxService : IOutboxService
	{
		private readonly ILogger<OutboxService> _logger;
		private readonly IOutboxRepository _outboxRepository;
		private readonly IOutboxMessagePublisher _outboxMessagePublisher;

		public OutboxService(
			ILogger<OutboxService> logger,
			IOutboxRepository outboxRepository,
			IOutboxMessagePublisher outboxMessagePublisher)
		{
			this._logger = logger;
			this._outboxRepository = outboxRepository;
			this._outboxMessagePublisher = outboxMessagePublisher;
		}

		public async Task ProcessOutbox(CancellationToken cancellationToken)
		{
			var messages = await this._outboxRepository.GetUnpublishedOutboxMessages(batchSize: 10, cancellationToken);

			this._logger.LogInformation("Outbox service found {MessageCount} unpublished outbox messages.", messages.Count);

			foreach (var message in messages)
			{
				try
				{
					this._logger.LogInformation("Dispatching outbox message with ID {OutboxMessageId} and type {MessageType}.", message.OutboxMessageId, message.MessageType);
					await this._outboxMessagePublisher.Publish(message, cancellationToken);

					this._logger.LogInformation("Marking outbox message with ID {OutboxMessageId} as published.", message.OutboxMessageId);
					await this._outboxRepository.MarkOutboxMessagePublished(message.OutboxMessageId, DateTimeOffset.UtcNow, cancellationToken);
				}
				catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
				{
					// Don't swallow cancellation exceptions when the application is shutting down
					throw;
				}
				catch (Exception ex)
				{
					// Log it, leaving PublishedAt as null so that it will be retried in the next iteration
					this._logger.LogError(ex, "Failed to dispatch outbox message with ID {OutboxMessageId}. It will be retried.", message.OutboxMessageId);
				}
			}

		}
	}

}
