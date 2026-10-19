CREATE TABLE dbo.LearnerCompletedCourse (
	LearnerId bigint NOT NULL,
	CourseId bigint NOT NULL,

	CONSTRAINT PK_LearnerCompletedCourse PRIMARY KEY CLUSTERED (LearnerId, CourseId),
	CONSTRAINT FK_LearnerCompletedCourse_Learner FOREIGN KEY (LearnerId) REFERENCES dbo.Learner (LearnerId),
	CONSTRAINT FK_LearnerCompletedCourse_Course FOREIGN KEY (CourseId) REFERENCES dbo.Course (CourseId),
)
