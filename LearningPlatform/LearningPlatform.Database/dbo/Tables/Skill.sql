CREATE TABLE dbo.Skill (
	SkillId bigint NOT NULL,
	SkillName varchar(100) NOT NULL,

	CONSTRAINT PK_Skill PRIMARY KEY CLUSTERED (SkillId),
	CONSTRAINT UQ_Skill_SkillName UNIQUE (SkillName),
)
