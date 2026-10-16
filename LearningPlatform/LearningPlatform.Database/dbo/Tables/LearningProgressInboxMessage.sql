CREATE TABLE dbo.LearningProgressInboxMessage (
    InboxSequence bigint IDENTITY NOT NULL,
    MessageId uniqueidentifier NOT NULL,
    ProcessedAt datetimeoffset NOT NULL,

    CONSTRAINT PK_LearningProgressInboxMessage PRIMARY KEY NONCLUSTERED (MessageId),
    CONSTRAINT UQ_LearningProgressInboxMessage_InboxSequence UNIQUE CLUSTERED (InboxSequence),
)
