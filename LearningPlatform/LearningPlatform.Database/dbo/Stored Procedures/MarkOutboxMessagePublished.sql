CREATE PROCEDURE dbo.MarkOutboxMessagePublished
    @OutboxMessageId uniqueidentifier,
    @PublishedAt datetimeoffset
AS
BEGIN

    UPDATE dbo.OutboxMessage
    SET PublishedAt = @PublishedAt
    WHERE
        OutboxMessageId = @OutboxMessageId AND
        PublishedAt IS NULL

END
GO
GRANT EXECUTE ON OBJECT::dbo.MarkOutboxMessagePublished TO [uami-learningplatform-outbox-aks] AS dbo
GO
GRANT EXECUTE ON OBJECT::dbo.MarkOutboxMessagePublished TO [learning-platform-docker-local] AS dbo
GO
GRANT EXECUTE ON OBJECT::dbo.MarkOutboxMessagePublished TO [ca-learningplatform-outbox] AS dbo
GO
GRANT EXECUTE ON OBJECT::dbo.MarkOutboxMessagePublished TO [app-learningplatform-api] AS dbo
GO
