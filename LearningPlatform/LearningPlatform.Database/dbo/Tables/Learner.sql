CREATE TABLE dbo.Learner (
	LearnerId bigint NOT NULL,
	TargetRoleId bigint NOT NULL,

	CONSTRAINT PK_Learner PRIMARY KEY CLUSTERED (LearnerId),
	CONSTRAINT FK_Learner_TargetRole FOREIGN KEY (TargetRoleId) REFERENCES dbo.Role (RoleId)
)
