# =================================================================================
# Multi-stage Dockerfile for creating the LearningPlatform API images
# =================================================================================
#
#  Stage 1 (ui-build)       ┌──────────────────────┐
#                           │ node:22-alpine       │
#                           │                      │
#                           │ npm ci               │
#                           │ npm run build        │
#                           └──────────┬───────────┘
#                                      │
#                                      │ wwwroot only
#                                      ▼
#  Stage 2 (dotnet-build)   ┌──────────────────────┐
#                           │ dotnet/sdk:10.0      │
#                           │                      │
#                           │ restore              │
#                           │ publish              │
#                           └──────────┬───────────┘
#                                      │
#                                      │ published app only
#                                      ▼
#  Stage 3 (runtime-base)   ┌──────────────────────┐
#                           │ dotnet/aspnet:10.0   │
#                           │                      │
#                           │ ASP.NET runtime      │
#                           │ published API + UI   │
#                           └──────────┬───────────┘
#                                      │
#                         ┌────────────┴────────────┐
#                         │                         │
#                         ▼                         ▼
#  Stage 4 (monolith)      ┌─────────────────┐     ┌─────────────────┐
#  Stage 5 (api-only)      │ API             │     │ API             │
#                          │ Outbox          │     │ only            │
#                          │ Consumer        │     │                 │
#                          └─────────────────┘     └─────────────────┘
#
# Execute with:
#   cd C:\Projects\Sleek\gspt-learning-platform
#   docker build -f Deployment\Docker\Dockerfile --target monolith -t learning-platform-api-monolith:phase13 .
#   docker build -f Deployment\Docker\Dockerfile --target api-only -t learning-platform-api-only:phase13 .
#
#       -f = the Dockerfile to use for the build
#       -t = tag the image (image/repo-name:tag)
#       .  = use this directory (i.e., the current directory) as the Docker build context.
#
# Run the monolithic API image:
#
#   docker run ^
#       --name learning-platform-api-monolith ^
#       -p 8080:8080 ^
#       --env-file Deployment\Docker\learning-platform.env ^
#       --env-file Deployment\Docker\learning-platform.secrets.env ^
#       learning-platform-api-monolith:phase13
#
# Run the API-only image:
#
#   docker run ^
#       --name learning-platform-api-only ^
#       -p 8080:8080 ^
#       --env-file Deployment\Docker\learning-platform.env ^
#       --env-file Deployment\Docker\learning-platform.secrets.env ^
#       learning-platform-api-only:phase13
#
# The decomposed architecture is normally started with:
#
#   docker compose -f Deployment\Docker\compose.yaml up

# ------------------------------------------------------------
# Stage 1 - Build the React application
# ------------------------------------------------------------

# The `FROM` instruction starts a new build stage, and specifies the base image for the subsequent instructions.
# The `AS ui-build` syntax gives a name (ui-build) to this build stage, which can be referenced in later stages.
# In this case, the base image is `node:22-alpine`, which is a lightweight version of Node.js based on Alpine Linux.
# This image is used to build the React application in the first stage of the multi-stage Docker build process.
# It already has Linux, Node.js, npm, etc. installed, which are necessary for building the React application.
FROM node:22-alpine AS ui-build

# The `WORKDIR` instruction sets the working directory for any subsequent instructions that follow.
# As if Docker created the /src directory and then changed the current working directory to /src.
WORKDIR /src

# Copy package.json and package-lock.json from the React project folder first, so that npm install can be run before copying the rest of the source code.
COPY    LearningPlatform/learning-platform-ui/package.json \
        LearningPlatform/learning-platform-ui/package-lock.json \
        ./learning-platform-ui/

# Run npm ci from the React folder.
# npm ci is like npm install, but it is meant to be used in automated environments such as continuous integration and deployment pipelines.
# For example, it always deletes the node_modules folder and installs dependencies from scratch.
# It installs dependencies based on the package-lock.json file, ensuring a clean and reproducible build environment.
RUN cd learning-platform-ui && \
    npm ci

# Copy the whole React project source code into the working directory (respecting .dockerignore)
COPY LearningPlatform/learning-platform-ui/ ./learning-platform-ui/

# vite.config.ts writes to ../LearningPlatform.EnrollmentApi/wwwroot.
# Navigate to the React project folder at /src/learning-platform-ui.
# Invoke vite to build the React application for production.
RUN mkdir -p ./LearningPlatform.EnrollmentApi/wwwroot && \
    cd learning-platform-ui && \
    npm run build


# Stage 1 result:
#   
#   ui-build stage
#   
#   /src
#   ├── learning-platform-ui
#   │   ├── source
#   │   ├── package.json
#   │   └── node_modules
#   │
#   └── LearningPlatform.EnrollmentApi
#       └── wwwroot
#           ├── index.html
#           └── assets
#               └── ...
#
# None of this stage will ultimately run in production.
# Node, npm, Vite, node_modules, and the React source are just construction equipment.


# ------------------------------------------------------------
# Stage 2 - Build and publish the ASP.NET Core application
# ------------------------------------------------------------

# Create the dotnet-build stage.
# This will build and publish the ASP.NET Core application using the official .NET SDK image.
# This image has the .NET SDK installed, which includes the compiler, dotnet restore, dotnet build, and dotnet publish commands, among other tools.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dotnet-build

# Set the working directory for the dotnet-build stage to /src.
WORKDIR /src


# Copy the .NET solution project files required for restoring NuGet packages.
COPY LearningPlatform/LearningPlatform.BusinessLogic/LearningPlatform.BusinessLogic.csproj      ./LearningPlatform.BusinessLogic/
COPY LearningPlatform/LearningPlatform.DataLayer/LearningPlatform.DataLayer.csproj              ./LearningPlatform.DataLayer/
COPY LearningPlatform/LearningPlatform.EnrollmentApi/LearningPlatform.EnrollmentApi.csproj      ./LearningPlatform.EnrollmentApi/
COPY LearningPlatform/LearningPlatform.Infrastructure/LearningPlatform.Infrastructure.csproj    ./LearningPlatform.Infrastructure/
COPY LearningPlatform/LearningPlatform.Shared/LearningPlatform.Shared.csproj                    ./LearningPlatform.Shared/

# Restore NuGet packages for the API project graph.
# Download and install any dependencies specified in LearningPlatform.EnrollmentApi.csproj (and all referenced projects), into the local NuGet package cache.
# Although dotnet publish normally does this implicitly before building, we are doing it here first as an independent step that can be cached.
RUN dotnet restore LearningPlatform.EnrollmentApi/LearningPlatform.EnrollmentApi.csproj

# Now copy the complete .NET application source code required by the API.
COPY LearningPlatform/LearningPlatform.BusinessLogic/   ./LearningPlatform.BusinessLogic/
COPY LearningPlatform/LearningPlatform.DataLayer/       ./LearningPlatform.DataLayer/
COPY LearningPlatform/LearningPlatform.EnrollmentApi/   ./LearningPlatform.EnrollmentApi/
COPY LearningPlatform/LearningPlatform.Infrastructure/  ./LearningPlatform.Infrastructure/
COPY LearningPlatform/LearningPlatform.Shared/          ./LearningPlatform.Shared/

# Copy the freshly built React application (from the ui-build stage) into the API project
COPY --from=ui-build /src/LearningPlatform.EnrollmentApi/wwwroot ./LearningPlatform.EnrollmentApi/wwwroot

# Publish the API project.
# The -c switch says to use release configuration.
# The -o switch says to save the deployable artifacts in /app/publish.
# The --no-restore switch says to skip restoring NuGet packages, since we just explicitly restored (and cached) them.
RUN dotnet publish \
    LearningPlatform.EnrollmentApi/LearningPlatform.EnrollmentApi.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


# ------------------------------------------------------------
# Stage 3 - Runtime base stage
# ------------------------------------------------------------

# Create the shared runtime base stage.
# This stage contains the ASP.NET Core runtime plus the published LearningPlatform API and React UI.
# It is not intended to be built as the final deployment image by itself; instead, the monolith
# and api-only stages both inherit from it and provide different hosting configurations.
# The ASP.NET image includes the ASP.NET Core runtime required to execute the published API.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime-base

# Set the working directory for the runtime base stage to /app.
WORKDIR /app

# Copy just the published output from the dotnet-build stage into the current /app directory of this runtime base stage.
COPY --from=dotnet-build /app/publish .

# Define an environment variable that tells ASP.NET Core to listen for HTTP traffic on port 8080 inside the container.
ENV ASPNETCORE_HTTP_PORTS=8080

# This image expects to serve traffic on port 8080.
EXPOSE 8080

# Specify the command that runs when a container is started from this image.
# This is equivalent conceptually to "dotnet LearningPlatform.EnrollmentApi.dll", but executed inside Linux.
# The square-bracket syntax is the exec form. Docker invokes the executable directly rather than running it through a shell, which is generally preferred for the main container process.
ENTRYPOINT ["dotnet", "LearningPlatform.EnrollmentApi.dll"]


# ------------------------------------------------------------
# Stage 4 - Monolithic API image
# ------------------------------------------------------------

# Create the monolith image from the shared runtime base. This version runs the API plus both background services.
FROM runtime-base AS monolith

# Enable the background services so that the API process also runs the OutboxDispatcher and LearningProgressConsumer.
ENV Hosting__RunBackgroundServices=true


# ------------------------------------------------------------
# Stage 5 - API-only (decomposed) image
# ------------------------------------------------------------

# Create the API-only image from the shared runtime base. This version runs only the API.
FROM runtime-base AS api-only

# Disable the background services so that the API process does not run the OutboxDispatcher and LearningProgressConsumer.
ENV Hosting__RunBackgroundServices=false
