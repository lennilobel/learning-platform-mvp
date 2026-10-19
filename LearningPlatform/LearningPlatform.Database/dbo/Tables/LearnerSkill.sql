CREATE TABLE dbo.LearnerSkill (
	LearnerId bigint NOT NULL,
	SkillId bigint NOT NULL,

	CONSTRAINT PK_LearnerSkill PRIMARY KEY CLUSTERED (LearnerId, SkillId),
	CONSTRAINT FK_LearnerSkill_Learner FOREIGN KEY (LearnerId) REFERENCES dbo.Learner (LearnerId),
	CONSTRAINT FK_LearnerSkill_Skill FOREIGN KEY (SkillId) REFERENCES dbo.Skill (SkillId),
)
