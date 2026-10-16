CREATE TABLE dbo.LearningProgress (
    LearnerId bigint NOT NULL,
    CourseId bigint NOT NULL,
    EnrolledAt datetimeoffset NOT NULL,

    CONSTRAINT PK_LearningProgress PRIMARY KEY CLUSTERED (LearnerId, CourseId),
)
