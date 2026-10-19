
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
GO
GRANT EXECUTE ON OBJECT::dbo.[GetUnpublishedOutboxMessages] TO [uami-learningplatform-outbox-aks] AS dbo
GO
GRANT EXECUTE ON OBJECT::dbo.[GetUnpublishedOutboxMessages] TO [learning-platform-docker-local] AS dbo
GO
GRANT EXECUTE ON OBJECT::dbo.[GetUnpublishedOutboxMessages] TO [ca-learningplatform-outbox] AS dbo
GO
GRANT EXECUTE ON OBJECT::dbo.[GetUnpublishedOutboxMessages] TO [app-learningplatform-api] AS dbo
GO
