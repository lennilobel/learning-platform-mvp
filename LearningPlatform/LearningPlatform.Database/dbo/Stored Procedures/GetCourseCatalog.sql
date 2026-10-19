CREATE PROCEDURE dbo.GetCourseCatalog
AS
BEGIN

	SELECT
		c.CourseId,
		c.Title,
		Role = r.RoleName,
		c.IsActive,
		PrerequisiteCourseIds = COALESCE((
			SELECT JSON_ARRAYAGG(cp.PrerequisiteCourseId ORDER BY cp.PrerequisiteCourseId)
			FROM dbo.CoursePrerequisite AS cp
			WHERE cp.CourseId = c.CourseId
		), '[]'),
		Skills = COALESCE((
			SELECT JSON_ARRAYAGG(s.SkillName ORDER BY s.SkillName)
			FROM
				dbo.CourseSkill AS cs
				INNER JOIN dbo.Skill AS s ON s.SkillId = cs.SkillId
			WHERE cs.CourseId = c.CourseId
		), '[]')
	FROM
		dbo.Course AS c
		INNER JOIN dbo.Role AS r ON r.RoleId = c.RoleId
	ORDER BY
		c.CourseId

END
GO
GRANT EXECUTE ON OBJECT::dbo.GetCourseCatalog TO [uami-learningplatform-api-aks] AS dbo
GO
GRANT EXECUTE ON OBJECT::dbo.GetCourseCatalog TO [learning-platform-docker-local] AS dbo
GO
GRANT EXECUTE ON OBJECT::dbo.GetCourseCatalog TO [ca-learningplatform-api] AS dbo
GO
GRANT EXECUTE ON OBJECT::dbo.GetCourseCatalog TO [app-learningplatform-api] AS dbo
GO
