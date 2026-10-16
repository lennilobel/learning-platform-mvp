using LearningPlatform.DataLayer.Outbox;

namespace LearningPlatform.BusinessLogic.Outbox
{
	// This interface defines a contract for publishing outbox messages to an external system (e.g., a message broker or event bus).
	public interface IOutboxMessagePublisher
	{
		Task Publish(OutboxMessage outboxMessage, CancellationToken cancellationToken = default);
	}
}
