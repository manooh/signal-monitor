# Presentation Notes

## Slides

URL:
```text
http://localhost:3030
```

```bash
#start w/ Docker
docker compose -f presentation/docker-compose.yml up
#stop
docker compose -f presentation/docker-compose.yml down
# stop & reset
docker compose -f presentation/docker-compose.yml down -v
```

## Demo Flow

1. Slides
2. Run the tests.
3. Start the API with Docker Compose.
4. Open Swagger UI.
5. Create a deployment.
6. List/filter deployments.
7. Update deployment status.
8. Delete deployment.
9. Show `/health`.
10. Briefly show the code structure in VS Code.

## .NET Commands

```bash
# Trust the local HTTPS development certificate, if needed
dotnet dev-certs https --trust

# Restore dependencies
dotnet restore trackr.sln

# Build the solution
dotnet build trackr.sln

# Run all tests
dotnet test trackr.sln

# Start the API directly with .NET
dotnet run --project src/Trackr.Api/Trackr.Api.csproj

# Start the API with the HTTPS launch profile and file watching
dotnet watch run --launch-profile https --project src/Trackr.Api/Trackr.Api.csproj

# Check Swagger and health in the browser:
# http://localhost:5000/swagger
# https://localhost:5001/swagger
# http://localhost:5000/health
# https://localhost:5001/health
```

## Docker Commands

```bash
# Build and start the API with Docker Compose
docker compose up --build

# Build and start the API in the background
docker compose up --build -d

# Check running containers
docker ps

# Show API logs
docker compose logs -f trackr-api

# Stop the API
docker compose down

# Check Swagger and health in the browser:
# http://localhost:5000/swagger
# http://localhost:5000/health
```

## API Commands

List deployments:

```bash
curl http://localhost:5000/api/deployments | jq
```

Filter deployments:

```bash
curl "http://localhost:5000/api/deployments?environment=prod" | jq
curl "http://localhost:5000/api/deployments?serviceName=trackr-api&status=Succeeded" | jq
```

Create deployment:

```bash
curl -i -X POST http://localhost:5000/api/deployments \
  -H "Content-Type: application/json" \
  -d '{
    "serviceName": "trackr-api",
    "environment": "dev",
    "version": "1.0.1",
    "status": "Queued",
    "deployedBy": "manuela",
    "commitSha": "abc123",
    "notes": "Testing from curl"
  }'
```

Update status:

```bash
curl -i -X PUT http://localhost:5000/api/deployments/YOUR-ID-HERE/status \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Succeeded",
    "notes": "Deployment completed"
  }'
```

Delete deployment:

```bash
curl -i -X DELETE http://localhost:5000/api/deployments/YOUR-ID-HERE
```

## Quick Recovery

```bash
# Check running containers
docker ps

# Check Docker Compose service status
docker compose ps

# Follow API logs
docker compose logs -f trackr-api

# Stop Docker Compose services
docker compose down

# Check what uses the API ports
lsof -i :5000
lsof -i :5001

# Kill a stuck local process after copying the PID from lsof
kill <PID>

# Check whether the API responds
curl -i http://localhost:5000/health
curl -k -i https://localhost:5001/health

# Check .NET environment and installed SDKs
dotnet --info

# Check repository status
git status --short

# Review local changes
git diff -- presentation/notes.md

# Check for whitespace problems before committing
git diff --check
```

## Create Project From Scratch

If only the C# and JSON source files were available, these are the commands to recreate the .NET solution, project setup, test setup, and Docker files around them.

```bash
# Create solution and project folders
mkdir trackr
cd trackr

dotnet new sln --name trackr
dotnet new webapi --name Trackr.Api --output src/Trackr.Api --framework net8.0
dotnet new xunit --name Trackr.Api.Tests --output tests/Trackr.Api.Tests --framework net8.0

# Remove template sample files that are replaced by the saved source files
rm -f src/Trackr.Api/WeatherForecast.cs
rm -f src/Trackr.Api/Trackr.Api.http
rm -f tests/Trackr.Api.Tests/UnitTest1.cs

# Add projects to the solution
dotnet sln trackr.sln add src/Trackr.Api/Trackr.Api.csproj
dotnet sln trackr.sln add tests/Trackr.Api.Tests/Trackr.Api.Tests.csproj

# Add API dependencies
dotnet remove src/Trackr.Api/Trackr.Api.csproj package Microsoft.AspNetCore.OpenApi
dotnet add src/Trackr.Api/Trackr.Api.csproj package Swashbuckle.AspNetCore --version 10.1.7

# Add test dependencies and connect tests to the API project
dotnet add tests/Trackr.Api.Tests/Trackr.Api.Tests.csproj reference src/Trackr.Api/Trackr.Api.csproj
dotnet add tests/Trackr.Api.Tests/Trackr.Api.Tests.csproj package FluentAssertions --version 6.12.2
dotnet add tests/Trackr.Api.Tests/Trackr.Api.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing --version 8.0.20
dotnet add tests/Trackr.Api.Tests/Trackr.Api.Tests.csproj package Microsoft.NET.Test.Sdk --version 17.8.0
dotnet add tests/Trackr.Api.Tests/Trackr.Api.Tests.csproj package xunit --version 2.5.3
dotnet add tests/Trackr.Api.Tests/Trackr.Api.Tests.csproj package xunit.runner.visualstudio --version 2.5.3
dotnet add tests/Trackr.Api.Tests/Trackr.Api.Tests.csproj package coverlet.collector --version 6.0.0

# Recreate the source layout before copying in the saved .cs/.json files
mkdir -p src/Trackr.Api/Controllers
mkdir -p src/Trackr.Api/Models
mkdir -p src/Trackr.Api/Repositories
mkdir -p src/Trackr.Api/Properties

# Copy/restore the saved files into these paths:
# src/Trackr.Api/Program.cs
# src/Trackr.Api/Controllers/DeploymentsController.cs
# src/Trackr.Api/Models/*.cs
# src/Trackr.Api/Repositories/*.cs
# src/Trackr.Api/appsettings.json
# src/Trackr.Api/Properties/launchSettings.json
# tests/Trackr.Api.Tests/*.cs

# Enable XML docs in the API project if the copied project file does not already have it
perl -0pi -e 's#<Nullable>enable</Nullable>#<Nullable>enable</Nullable>\n    <GenerateDocumentationFile>true</GenerateDocumentationFile>#' \
  src/Trackr.Api/Trackr.Api.csproj

# Create Dockerfile
cat > Dockerfile <<'EOF'
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Trackr.Api/Trackr.Api.csproj src/Trackr.Api/
RUN dotnet restore src/Trackr.Api/Trackr.Api.csproj

COPY src/Trackr.Api/ src/Trackr.Api/
RUN dotnet publish src/Trackr.Api/Trackr.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Trackr.Api.dll"]
EOF

# Create Docker Compose setup
cat > docker-compose.yml <<'EOF'
services:
  trackr-api:
    build:
      context: .
      dockerfile: Dockerfile
    image: trackr-api:local
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:8080
    ports:
      - "5000:8080"
EOF

# Restore, build, test, and run
dotnet restore trackr.sln
dotnet build trackr.sln
dotnet test trackr.sln
docker compose up --build
```
