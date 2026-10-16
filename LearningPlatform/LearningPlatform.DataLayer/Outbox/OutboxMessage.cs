namespace LearningPlatform.DataLayer.Outbox
{
	public class OutboxMessage
	{
		public long OutboxSequence { get; set; }
		public Guid OutboxMessageId { get; set; }
		public string MessageType { get; set; } = null!;
		public string Payload { get; set; } = null!;
		public DateTimeOffset OccurredAt { get; set; }
	}
}
