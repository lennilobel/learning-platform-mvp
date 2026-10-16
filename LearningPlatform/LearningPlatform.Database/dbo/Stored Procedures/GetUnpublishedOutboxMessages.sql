CREATE PROCEDURE dbo.GetUnpublishedOutboxMessages
    @BatchSize int
AS
BEGIN

    SELECT TOP (@BatchSize)
        OutboxSequence,
        OutboxMessageId,
        MessageType,
        Payload = CONVERT(nvarchar(max), Payload),
        OccurredAt
    FROM dbo.OutboxMessage
    WHERE PublishedAt IS NULL
    ORDER BY OutboxSequence

END
