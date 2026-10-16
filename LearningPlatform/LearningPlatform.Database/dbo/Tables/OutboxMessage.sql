CREATE TABLE dbo.OutboxMessage (
    OutboxSequence bigint IDENTITY NOT NULL,
    OutboxMessageId uniqueidentifier NOT NULL,
    MessageType varchar(100) NOT NULL,
    Payload json NOT NULL,
    OccurredAt datetimeoffset NOT NULL,
    PublishedAt datetimeoffset,

    CONSTRAINT PK_OutboxMessage PRIMARY KEY NONCLUSTERED (OutboxMessageId),
    CONSTRAINT UQ_OutboxMessage_OutboxSequence UNIQUE CLUSTERED (OutboxSequence),
)
