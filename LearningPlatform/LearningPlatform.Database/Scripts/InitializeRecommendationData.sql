SET XACT_ABORT ON

BEGIN TRANSACTION

	-- Clear enrollment/event-processing state.

	DELETE FROM dbo.LearningProgressInboxMessage
	DELETE FROM dbo.LearningProgress
	DELETE FROM dbo.OutboxMessage
	DELETE FROM dbo.Enrollment

	-- Clear recommendation data in foreign-key-safe order.

	DELETE FROM dbo.LearnerCompletedCourse
	DELETE FROM dbo.CoursePrerequisite
	DELETE FROM dbo.CourseSkill
	DELETE FROM dbo.LearnerSkill
	DELETE FROM dbo.Learner
	DELETE FROM dbo.Course
	DELETE FROM dbo.Skill
	DELETE FROM dbo.Role

	-- Roles

	INSERT INTO dbo.Role (RoleId, RoleName)
	VALUES
		(1, 'Azure Developer'),
		(2, 'Data Analyst')

	-- Skills

	INSERT INTO dbo.Skill (SkillId, SkillName)
	VALUES
		(1, 'C#'),
		(2, 'ASP.NET Core'),
		(3, 'Azure'),
		(4, 'App Service'),
		(5, 'AKS'),
		(6, 'Kubernetes'),
		(7, 'Power BI'),
		(8, 'Data Visualization')

	-- Learners

	INSERT INTO dbo.Learner (LearnerId, TargetRoleId)
	VALUES
		(1, 1)

	-- Learner skills

	INSERT INTO dbo.LearnerSkill (LearnerId, SkillId)
	VALUES
		(1, 1),	-- C#
		(1, 2)	-- ASP.NET Core

	-- Courses
	--
	-- Course 999 exists only to preserve referential integrity for the
	-- Advanced AKS prerequisite used by the original demo data.
	-- It is inactive so it cannot itself become a recommendation.

	INSERT INTO dbo.Course (CourseId, Title, RoleId, IsActive)
	VALUES
		(100, 'C# Fundamentals',			1, 1),
		(200, 'Azure App Service',			1, 1),
		(300, 'Advanced AKS',				1, 1),
		(400, 'Power BI Fundamentals',		2, 1),
		(999, 'Kubernetes Fundamentals',	1, 0)

	-- Course skills

	INSERT INTO dbo.CourseSkill (CourseId, SkillId)
	VALUES
		(100, 1),	-- C#

		(200, 3),	-- Azure
		(200, 4),	-- App Service

		(300, 5),	-- AKS
		(300, 6),	-- Kubernetes

		(400, 7),	-- Power BI
		(400, 8)	-- Data Visualization

	-- Course prerequisites

	INSERT INTO dbo.CoursePrerequisite (CourseId, PrerequisiteCourseId)
	VALUES
		(200, 100),
		(300, 999)

	-- Completed courses

	INSERT INTO dbo.LearnerCompletedCourse (LearnerId, CourseId)
	VALUES
		(1, 100)

COMMIT TRANSACTION
