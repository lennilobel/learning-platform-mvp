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
