-- This must be run on an Entra-authenticated connection with sufficient permission to create external users

----------------------------------------------------------------------------------------------------------------------------
-- For the App Service
CREATE USER [app-learningplatform-api] FROM EXTERNAL PROVIDER
GO

GRANT EXECUTE ON OBJECT::dbo.CreateEnrollment TO [app-learningplatform-api]
GRANT EXECUTE ON OBJECT::dbo.GetUnpublishedOutboxMessages TO [app-learningplatform-api]
GRANT EXECUTE ON OBJECT::dbo.MarkOutboxMessagePublished TO [app-learningplatform-api]
GRANT EXECUTE ON OBJECT::dbo.ProcessEnrollmentCreated TO [app-learningplatform-api]
GO


----------------------------------------------------------------------------------------------------------------------------
-- For the Docker container
CREATE USER [learning-platform-docker-local] FROM EXTERNAL PROVIDER
GO

GRANT EXECUTE ON OBJECT::dbo.CreateEnrollment TO [learning-platform-docker-local]
GRANT EXECUTE ON OBJECT::dbo.GetUnpublishedOutboxMessages TO [learning-platform-docker-local]
GRANT EXECUTE ON OBJECT::dbo.MarkOutboxMessagePublished TO [learning-platform-docker-local]
GRANT EXECUTE ON OBJECT::dbo.ProcessEnrollmentCreated TO [learning-platform-docker-local]
GO


----------------------------------------------------------------------------------------------------------------------------
-- For the Azure Function
CREATE USER [func-learningplatform-learningprogress-uami] FROM EXTERNAL PROVIDER
GO

GRANT EXECUTE ON OBJECT::dbo.ProcessEnrollmentCreated TO [func-learningplatform-learningprogress-uami]
GO


----------------------------------------------------------------------------------------------------------------------------
-- For Azure Container Applications (ACA)

-- ACA API workload identity
CREATE USER [ca-learningplatform-api] FROM EXTERNAL PROVIDER
GO

GRANT EXECUTE ON OBJECT::dbo.CreateEnrollment TO [ca-learningplatform-api]
GO

-- ACA Outbox workload identity
CREATE USER [ca-learningplatform-outbox] FROM EXTERNAL PROVIDER
GO

GRANT EXECUTE ON OBJECT::dbo.GetUnpublishedOutboxMessages TO [ca-learningplatform-outbox]
GRANT EXECUTE ON OBJECT::dbo.MarkOutboxMessagePublished TO [ca-learningplatform-outbox]
GO

-- ACA Progress workload identity
CREATE USER [ca-learningplatform-progress] FROM EXTERNAL PROVIDER
GO

GRANT EXECUTE ON OBJECT::dbo.ProcessEnrollmentCreated TO [ca-learningplatform-progress]
GO


----------------------------------------------------------------------------------------------------------------------------
-- For Azure Kubernetes Services (AKS)

-- AKS API workload identity
CREATE USER [uami-learningplatform-api-aks] FROM EXTERNAL PROVIDER
GO

GRANT EXECUTE ON OBJECT::dbo.CreateEnrollment TO [uami-learningplatform-api-aks]
GO

-- AKS Outbox workload identity
CREATE USER [uami-learningplatform-outbox-aks] FROM EXTERNAL PROVIDER
GO

GRANT EXECUTE ON OBJECT::dbo.GetUnpublishedOutboxMessages TO [uami-learningplatform-outbox-aks]
GRANT EXECUTE ON OBJECT::dbo.MarkOutboxMessagePublished TO [uami-learningplatform-outbox-aks]
GO

-- AKS Progress workload identity
CREATE USER [uami-learningplatform-progress-aks] FROM EXTERNAL PROVIDER
GO

GRANT EXECUTE ON OBJECT::dbo.ProcessEnrollmentCreated TO [uami-learningplatform-progress-aks]
GO
