using Azure.Messaging.ServiceBus;
using LearningPlatform.BusinessLogic.LearningProgress;
using LearningPlatform.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LearningPlatform.LearningProgressFunction
{
	// This is the Function version of the LearningProgressConsumer background service. It is triggered by Service Bus messages and processes them using the ILearningProgressService.
	public class LearningProgressFunction
	{
		private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

		private readonly ILogger<LearningProgressFunction> _logger;
		private readonly ILearningProgressService _learningProgressService;

		public LearningProgressFunction(
			ILogger<LearningProgressFunction> logger,
			ILearningProgressService learningProgressService)
		{
			this._logger = logger;
			this._learningProgressService = learningProgressService;
		}

		[Function(nameof(LearningProgressFunction))]
		public async Task Run(
			[ServiceBusTrigger(
				"enrollment-created",
				"learning-progress",
				Connection = "ServiceBusConnection",
				AutoCompleteMessages = false)]	// we will manually complete the message after processing so that we explicitly control settlement and can test idempotent redelivery
			ServiceBusReceivedMessage message,
			ServiceBusMessageActions messageActions,
			CancellationToken cancellationToken)
		{
			// Process the delivered Service Bus message.
			var enrollmentCreatedEvent = JsonSerializer.Deserialize<EnrollmentCreatedEvent>(message.Body.ToString(), _jsonSerializerOptions)
				?? throw new InvalidOperationException("Learning progress function is unable to deserialize EnrollmentCreatedEvent.");

			this._logger.LogWarning("Learning progress function processing EnrollmentCreatedEvent SB message. EventId={EventId}, LearnerId={LearnerId}, CourseId={CourseId}",
				enrollmentCreatedEvent.EventId,
				enrollmentCreatedEvent.LearnerId,
				enrollmentCreatedEvent.CourseId);

			// Invoke the idempotent business operation; the business/data layers ensure that a redelivered Service Bus message does not repeat the business effect.
			var isDuplicate = await this._learningProgressService.ProcessEnrollmentCreated(enrollmentCreatedEvent, cancellationToken);

			this._logger.LogWarning("Processed EnrollmentCreated message. MessageId={MessageId}, DeliveryCount={DeliveryCount}, IsDuplicate={IsDuplicate}",
				message.MessageId,
				message.DeliveryCount,
				isDuplicate);

			// Simulate a crash after the SQL commit on the first delivery, but before the Service Bus Complete, to test the idempotency of the consumer for the subsequent delivery attempt.
			if (message.DeliveryCount == 1)
			{
				// Will force the message to be redelivered, which is why the consumer must be idempotent (i.e., it must not create duplicate learning progress records for the same enrollment event)
				throw new InvalidOperationException("Simulated crash after SQL commit (only on first delivery), before Service Bus Complete.");
			}

			// Complete the delivered message after processing (without this, the delivered message will be redelivered after the lock expires, or if/when the processor abandons/releases the message).
			await messageActions.CompleteMessageAsync(message, cancellationToken);

			this._logger.LogWarning("Learning progress function completed EnrollmentCreatedEvent SB message.");
		}
	}
}
