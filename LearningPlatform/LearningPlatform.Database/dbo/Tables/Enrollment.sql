CREATE TABLE dbo.Enrollment (
    LearnerId bigint NOT NULL,
    CourseId  bigint NOT NULL,

    CONSTRAINT PK_Enrollment PRIMARY KEY CLUSTERED (LearnerId, CourseId),
)
