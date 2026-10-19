CREATE TABLE dbo.CoursePrerequisite (
	CourseId bigint NOT NULL,
	PrerequisiteCourseId bigint NOT NULL,

	CONSTRAINT PK_CoursePrerequisite PRIMARY KEY CLUSTERED (CourseId, PrerequisiteCourseId),
	CONSTRAINT FK_CoursePrerequisite_Course FOREIGN KEY (CourseId) REFERENCES dbo.Course (CourseId),
	CONSTRAINT FK_CoursePrerequisite_PrerequisiteCourse FOREIGN KEY (PrerequisiteCourseId) REFERENCES dbo.Course (CourseId),
	CONSTRAINT CK_CoursePrerequisite_NotSelf CHECK (CourseId != PrerequisiteCourseId),
)
