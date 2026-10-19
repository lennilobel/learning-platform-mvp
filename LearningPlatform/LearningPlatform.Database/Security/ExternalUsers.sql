-- This must be run on an Entra-authenticated connection with sufficient permission to create external users

CREATE USER [app-learningplatform-api] FROM EXTERNAL PROVIDER						-- Azure App Service
GO
CREATE USER [learning-platform-docker-local] FROM EXTERNAL PROVIDER					-- Docker container
GO
CREATE USER [func-learningplatform-learningprogress-uami] FROM EXTERNAL PROVIDER	-- Azure Function
GO
CREATE USER [ca-learningplatform-api] FROM EXTERNAL PROVIDER						-- ACA API workload identity
GO
CREATE USER [ca-learningplatform-outbox] FROM EXTERNAL PROVIDER						-- ACA Outbox workload identity
GO
CREATE USER [ca-learningplatform-progress] FROM EXTERNAL PROVIDER					-- ACA Progress workload identity
GO
CREATE USER [uami-learningplatform-api-aks] FROM EXTERNAL PROVIDER					-- AKS API workload identity
GO
CREATE USER [uami-learningplatform-outbox-aks] FROM EXTERNAL PROVIDER				-- AKS Outbox workload identity
GO
CREATE USER [uami-learningplatform-progress-aks] FROM EXTERNAL PROVIDER				-- AKS Progress workload identity
GO
