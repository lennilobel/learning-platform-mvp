CREATE PROCEDURE dbo.ProcessEnrollmentCreated
    @MessageId uniqueidentifier,
    @LearnerId bigint,
    @CourseId bigint,
    @OccurredAt datetimeoffset
AS
BEGIN

    DECLARE @IsDuplicate bit = 0

    BEGIN TRY
        BEGIN TRANSACTION

            IF EXISTS (
                SELECT 1
                FROM dbo.LearningProgressInboxMessage WITH (UPDLOCK, HOLDLOCK)
                WHERE MessageId = @MessageId
            )
                SET @IsDuplicate = 1
            ELSE
            BEGIN
                INSERT INTO dbo.LearningProgress (LearnerId, CourseId, EnrolledAt)
                 VALUES (@LearnerId, @CourseId, @OccurredAt)

                INSERT INTO dbo.LearningProgressInboxMessage (MessageId, ProcessedAt)
                 VALUES (@MessageId, SYSDATETIMEOFFSET())
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
