# Learning Platform MVP

This solution showcases my full-stack .NET 10 learning-platform MVP (Minimum Viable Product).
I built this solution to demonstrate modern application architecture,
distributed systems, Azure cloud services, DevOps, security, and agentic AI.

It demonstrates modern application architecture, distributed systems,
cloud-native Azure hosting, secure identity, DevOps, deterministic recommendation logic,
and agentic AI.

The solution spans a React/TypeScript frontend, ASP.NET Core APIs,
SQL Server/Azure SQL persistence, asynchronous event processing,
multiple Azure hosting models, Infrastructure as Code, CI/CD,
observability, and Microsoft Foundry-based AI agents.

Key fundamentals demonstrated by the solution include:

- Application Architecture
  - Layered API, business logic, data access, infrastructure, and shared-contract projects
  - Dependency injection with scoped and singleton service lifetimes
  - Repository pattern
  - Application service orchestration
  - Async/await and cancellation-token propagation
  - Monolithic and decomposed/microservice hosting models
  - Shared business logic across API, Worker Service, and Azure Function hosts

- Distributed Architecture Patterns
  - Event-driven architecture
  - Transactional Outbox pattern
  - Idempotent Inbox/consumer pattern
  - At-least-once message delivery handling
  - Atomic business-data and outbox persistence
  - Azure Service Bus topics and subscriptions
  - Service Bus duplicate detection
  - Explicit message settlement
  - Message redelivery and simulated consumer-failure testing
  - Background outbox dispatcher
  - Dedicated outbox and learning-progress workers
  - Azure Function event consumer alternative

- Data and Persistence
  - SQL Server / Azure SQL Database
  - SQL Database Project
  - Normalized relational data model
  - Stored-procedure-based persistence
  - Explicit SQL transactions
  - Concurrency control with locking
  - Entity Framework Core
  - Keyless EF Core result types
  - Parameterized stored-procedure execution through EF Core
  - SQL JSON aggregation and deserialization
  - Managed-identity database authentication
  - Database-scoped users and least-privilege stored-procedure permissions

- Deterministic Recommendation Engine
  - Learner profiles and target roles
  - Learner skills and completed courses
  - Course skills and prerequisites
  - Active-course eligibility filtering
  - Completed-course exclusion
  - Prerequisite enforcement
  - Weighted course ranking
  - Target-role scoring
  - Skill-gap scoring
  - Explainable recommendation reasons
  - Pluggable learning-data repository abstraction
  - In-memory/demo and SQL-backed repository implementations

- Agentic AI
  - Microsoft Agent Framework
  - Microsoft Foundry project/model integration
  - Conversational recommendation agent
  - Tool/function calling
  - Tool metadata using descriptions
  - Learner-profile retrieval tool
  - Deterministic recommendation tool
  - Course-details retrieval tool
  - Grounding agent responses in deterministic application data and logic
  - Separation of deterministic recommendation logic from probabilistic agent orchestration
  - Read-only stateless HTTP agent
  - Side-effecting enrollment tool
  - Human-in-the-loop tool approval
  - Agent sessions for approval continuation
  - OpenTelemetry agent tracing
  - Custom OpenTelemetry activity exporter
  - Deterministic offline agent evaluation
  - Tool-call and tool-argument evaluation
  - Keyword/output evaluation
  - Custom evaluation checks
  - Evaluation pass/fail gating
  - Foundry semantic/LLM-as-judge evaluation scaffold
  - Online evaluation of live agent responses
  - Live-response and recommendation-tool-usage checks

- Web API
  - ASP.NET Core Web API
  - RESTful enrollment API
  - Deterministic recommendation API
  - Conversational recommendation-agent API
  - Controller/application-service separation
  - Server-side validation
  - Duplicate-enrollment conflict handling
  - OpenAPI support
  - CORS support
  - SPA static-file hosting and fallback routing

- React Frontend
  - React
  - TypeScript
  - Vite
  - React Router
  - MSAL React authentication
  - Enrollment workflow
  - Client-side validation
  - Async loading and success states
  - Deterministic recommendation UI
  - Conversational recommendation-agent UI
  - Markdown rendering of agent responses
  - Bearer-token acquisition and authenticated API calls

- Identity and Security
  - Microsoft Entra ID authentication
  - JWT bearer authentication
  - OAuth API scopes
  - MSAL browser authentication
  - DefaultAzureCredential
  - User-assigned managed identities
  - System-assigned managed identities
  - Azure RBAC
  - AKS Workload Identity
  - Federated identity credentials
  - Passwordless Service Bus authentication
  - Passwordless Azure SQL authentication
  - Foundry managed-identity access
  - GitHub-to-Azure OIDC federation
  - Least-privilege workload-specific identities

- Containers and Azure Hosting
  - Docker
  - Multi-container Docker Compose
  - Separate API, outbox-worker, and learning-progress-worker images
  - Azure Container Registry
  - Azure Kubernetes Service
  - Kubernetes Deployments and Services
  - Kubernetes ServiceAccounts
  - AKS OIDC issuer and Workload Identity
  - Azure Container Apps
  - Azure App Service
  - Azure Functions
  - Function Flex Consumption hosting
  - Application Insights
  - Log Analytics

- Infrastructure as Code
  - Azure Bicep
  - Modular Bicep architecture
  - AKS provisioning
  - Azure Container Registry provisioning
  - Service Bus provisioning
  - Managed-identity provisioning
  - Federated-credential provisioning
  - RBAC role assignments
  - App Service provisioning
  - Azure Functions provisioning
  - Azure Container Apps provisioning
  - Log Analytics and Application Insights integration

- CI/CD
  - GitHub Actions
  - .NET build and test automation
  - React build automation
  - Docker image build automation
  - ACR image publishing
  - Commit-specific container image tagging
  - Passwordless Azure login using GitHub OIDC
  - Automated AKS deployment
  - Kubernetes image updates
  - AKS rollout verification
  - CI on pull requests and pushes
  - CD from the main branch

- Observability and Diagnostics
  - Structured .NET logging
  - OpenTelemetry
  - Agent traces, spans, and tool-call telemetry
  - Azure Monitor OpenTelemetry exporter
  - Application Insights telemetry
  - Function telemetry using managed identity
  - Runtime processing and duplicate-detection logging

- Testing
  - xUnit
  - Moq
  - Business-service unit tests
  - Recommendation eligibility tests
  - Weighted-ranking tests
  - Recommendation orchestration tests
  - Mocked repository dependencies
  - Deterministic agent evaluation tests