CREATE PROCEDURE dbo.GetLearnerContext
	@LearnerId bigint
AS
BEGIN

	SELECT
		l.LearnerId,
		TargetRole = r.RoleName,
		CompletedCourseIds = ISNULL((
			SELECT JSON_ARRAYAGG(lcc.CourseId ORDER BY lcc.CourseId)
			FROM dbo.LearnerCompletedCourse AS lcc
			WHERE lcc.LearnerId = l.LearnerId
		), '[]'),
		Skills = ISNULL((
			SELECT JSON_ARRAYAGG(s.SkillName ORDER BY s.SkillName)
			FROM
				dbo.LearnerSkill AS ls
				INNER JOIN dbo.Skill AS s ON s.SkillId = ls.SkillId
			WHERE ls.LearnerId = l.LearnerId
		), '[]')
	FROM
		dbo.Learner AS l
		INNER JOIN dbo.Role AS r  ON r.RoleId = l.TargetRoleId
	WHERE
		l.LearnerId = @LearnerId

END
GO
GRANT EXECUTE ON OBJECT::dbo.GetLearnerContext TO [uami-learningplatform-api-aks] AS dbo
GO
GRANT EXECUTE ON OBJECT::dbo.GetLearnerContext TO [learning-platform-docker-local] AS dbo
GO
GRANT EXECUTE ON OBJECT::dbo.GetLearnerContext TO [ca-learningplatform-api] AS dbo
GO
GRANT EXECUTE ON OBJECT::dbo.GetLearnerContext TO [app-learningplatform-api] AS dbo
GO
