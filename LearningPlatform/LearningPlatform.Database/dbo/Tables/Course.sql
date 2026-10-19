CREATE TABLE dbo.Course (
	CourseId bigint NOT NULL,
	Title varchar(200) NOT NULL,
	RoleId bigint NOT NULL,
	IsActive bit NOT NULL CONSTRAINT DF_Course_IsActive DEFAULT (1),

	CONSTRAINT PK_Course PRIMARY KEY CLUSTERED (CourseId),
	CONSTRAINT FK_Course_Role FOREIGN KEY (RoleId) REFERENCES dbo.Role (RoleId),
)
