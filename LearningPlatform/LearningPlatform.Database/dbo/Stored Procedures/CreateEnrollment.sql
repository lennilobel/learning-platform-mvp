CREATE PROCEDURE dbo.CreateEnrollment
    -- Enrollment table parameters
    @LearnerId bigint,
    @CourseId bigint,
    -- Outbox table parameters
    @EventId uniqueidentifier,
    @MessageType varchar(100),
    @Payload nvarchar(max),     -- could/should be json data type, but C# SqlParameter may not support it, so we use nvarchar(max) instead and convert to json internally
    @OccurredAt datetimeoffset
AS
BEGIN

    DECLARE @IsDuplicate bit = 0

    BEGIN TRY
        BEGIN TRANSACTION
            IF EXISTS (
                SELECT 1
                FROM dbo.Enrollment WITH (UPDLOCK, HOLDLOCK)    -- Update/range locks held through transaction to prevent check and insert race
                WHERE LearnerId = @LearnerId AND CourseId = @CourseId
            )
                SET @IsDuplicate = 1
            ELSE
            BEGIN
                INSERT INTO dbo.Enrollment (LearnerId, CourseId)
                 VALUES (@LearnerId, @CourseId)

                INSERT INTO dbo.OutboxMessage (OutboxMessageId, MessageType, Payload, OccurredAt)
                 VALUES (@EventId, @MessageType, CONVERT(json, @Payload), @OccurredAt)
            END
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        THROW
    END CATCH

    SELECT @IsDuplicate AS IsDuplicate

END
