using Azure.Messaging.ServiceBus;
using LearningPlatform.BusinessLogic.Outbox;
using LearningPlatform.DataLayer.Outbox;
using Microsoft.Extensions.Logging;

namespace LearningPlatform.Infrastructure
{
	public class ServiceBusEnrollmentEventPublisher : IOutboxMessagePublisher
	{
		private readonly ServiceBusSender _sender;
		private readonly ILogger<ServiceBusEnrollmentEventPublisher> _logger;

		public ServiceBusEnrollmentEventPublisher(ServiceBusSender sender, ILogger<ServiceBusEnrollmentEventPublisher> logger)
		{
			this._sender = sender;
			this._logger = logger;
		}

		public async Task Publish(OutboxMessage outboxMessage, CancellationToken cancellationToken = default)
		{
			var message = new ServiceBusMessage(BinaryData.FromString(outboxMessage.Payload))
			{
				MessageId = outboxMessage.OutboxMessageId.ToString(),
				Subject = outboxMessage.MessageType,
				ContentType = "application/json",
			};

			await this._sender.SendMessageAsync(message, cancellationToken);

			this._logger.LogWarning("Sent EnrollmentCreatedEvent SB message. OutboxMessageId={OutboxMessageId}", outboxMessage.OutboxMessageId);
		}

	}
}
