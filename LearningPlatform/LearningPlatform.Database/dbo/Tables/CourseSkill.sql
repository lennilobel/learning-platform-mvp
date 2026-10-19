CREATE TABLE dbo.CourseSkill (
	CourseId bigint NOT NULL,
	SkillId bigint NOT NULL,

	CONSTRAINT PK_CourseSkill PRIMARY KEY CLUSTERED (CourseId, SkillId),
	CONSTRAINT FK_CourseSkill_Course FOREIGN KEY (CourseId) REFERENCES dbo.Course (CourseId),
	CONSTRAINT FK_CourseSkill_Skill FOREIGN KEY (SkillId) REFERENCES dbo.Skill (SkillId),
)
