# Trackr

Trackr is a small C#/.NET 8 REST API for tracking deployment status across services and environments. It uses an in-memory repository seeded from `appsettings.json`, exposes Swagger/OpenAPI documentation, and includes focused repository and API integration tests.

## Project Structure

- `src/Trackr.Api` - ASP.NET Core Web API
- `src/Trackr.Api/Controllers` - HTTP endpoints
- `src/Trackr.Api/Models` - request, response, and enum types
- `src/Trackr.Api/Repositories` - deployment storage abstraction and in-memory implementation
- `tests/Trackr.Api.Tests` - xUnit tests for repository behavior and API endpoints

## Prerequisites

- .NET 8 SDK
- Docker, if you want to run the API in a container

## Build

```bash
dotnet build trackr.sln
```

## Run

```bash
dotnet run --project src/Trackr.Api/Trackr.Api.csproj
```

Run with the HTTPS launch profile:

```bash
dotnet run --project src/Trackr.Api/Trackr.Api.csproj --launch-profile https
```

The API starts on the URLs printed by `dotnet run`. With the default launch settings these are:

- `http://localhost:5000`
- `https://localhost:5001`

Swagger UI is available in development at:

```text
http://localhost:5000/swagger
```

## Docker

Run the API with Docker Compose:

```bash
docker compose up --build
```

This builds the image and starts the API at `http://localhost:5000`, with Swagger UI at `/swagger`.

Stop and remove the Compose container:

```bash
docker compose down
```

To build and run the image manually:

```bash
docker build -t trackr-api .
```

```bash
docker run --rm -p 5000:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  --name trackr-api \
  trackr-api
```

## Test

```bash
dotnet test trackr.sln
```

## API Endpoints

| Method | Path | Description |
| --- | --- | --- |
| `GET` | `/api/deployments` | List deployments, optionally filtered by `environment`, `serviceName`, or `status` |
| `GET` | `/api/deployments/{id}` | Get one deployment |
| `GET` | `/api/deployments/latest` | Get the latest deployment, optionally filtered by `environment` |
| `POST` | `/api/deployments` | Create a deployment |
| `PUT` | `/api/deployments/{id}/status` | Update deployment status and optional notes |
| `DELETE` | `/api/deployments/{id}` | Delete a deployment |
| `GET` | `/health` | Health check |

## Example Requests

List deployments:

```bash
curl http://localhost:5000/api/deployments
```

Create a deployment:

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

Update deployment status:

```bash
curl -i -X PUT http://localhost:5000/api/deployments/YOUR-ID-HERE/status \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Succeeded",
    "notes": "Deployment completed"
  }'
```

Delete a deployment:

```bash
curl -i -X DELETE http://localhost:5000/api/deployments/YOUR-ID-HERE
```

## Documentation

Swagger/OpenAPI is generated with Swashbuckle. XML documentation comments are enabled in the API project and included in Swagger, so public API descriptions appear in Swagger UI during development.

## Design Notes

- The in-memory repository keeps the prototype simple and easy to run locally.
- `IDeploymentRepository` keeps storage replaceable, for example with SQLite later.
- JSON enum values are serialized as strings, so requests can use values such as `"Queued"` or `"Succeeded"`.
- API integration tests cover routing, model validation, JSON options, dependency injection, and controller behavior.
