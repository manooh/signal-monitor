# Signal Monitor

Signal Monitor is a small C#/.NET 8 REST API for tracking servers, recording health signals, and raising simple alarms. It uses an in-memory repository seeded from `appsettings.json`, exposes Swagger/OpenAPI documentation, and includes focused repository and API integration tests.

## Project Structure

- `src/SignalMonitor.Api` - ASP.NET Core Web API
- `src/SignalMonitor.Api/Controllers` - HTTP endpoints
- `src/SignalMonitor.Api/Models` - request, response, and enum types
- `src/SignalMonitor.Api/Repositories` - monitoring storage abstraction and in-memory implementation
- `tests/SignalMonitor.Api.Tests` - xUnit tests for repository behavior and API endpoints

## Prerequisites

- .NET 8 SDK
- Docker, if you want to run the API in a container

## Build

```bash
dotnet build signal-monitor.sln
```

## Run

```bash
dotnet run --project src/SignalMonitor.Api/SignalMonitor.Api.csproj
```

Run with the HTTPS launch profile:

```bash
dotnet run --project src/SignalMonitor.Api/SignalMonitor.Api.csproj --launch-profile https
```

With the default launch settings, the API starts at:

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
docker build -t signal-monitor-api .
```

```bash
docker run --rm -p 5000:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  --name signal-monitor-api \
  signal-monitor-api
```

## Test

```bash
dotnet test signal-monitor.sln
```

## API Endpoints

| Method | Path | Description |
| --- | --- | --- |
| `GET` | `/api/servers` | List servers, optionally filtered by `environment` or `status` |
| `GET` | `/api/servers/{id}` | Get one server |
| `POST` | `/api/servers` | Register a server |
| `POST` | `/api/servers/{id}/signals` | Record a heartbeat, CPU, or memory signal |
| `DELETE` | `/api/servers/{id}` | Delete a server and its related data |
| `GET` | `/api/signals` | List signal samples, optionally filtered by `serverId` or `kind` |
| `GET` | `/api/alarms` | List alarms, optionally filtered by `serverId` or `status` |
| `PUT` | `/api/alarms/{id}/status` | Acknowledge or resolve an alarm |
| `GET` | `/health` | Health check |

## Example Requests

List servers:

```bash
curl http://localhost:5000/api/servers
```

Register a server:

```bash
curl -i -X POST http://localhost:5000/api/servers \
  -H "Content-Type: application/json" \
  -d '{
    "name": "api-prod-02",
    "environment": "production",
    "region": "eu-central"
  }'
```

Record a high CPU signal:

```bash
curl -i -X POST http://localhost:5000/api/servers/YOUR-SERVER-ID-HERE/signals \
  -H "Content-Type: application/json" \
  -d '{
    "kind": "Cpu",
    "value": 93
  }'
```

Resolve an alarm:

```bash
curl -i -X PUT http://localhost:5000/api/alarms/YOUR-ALARM-ID-HERE/status \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Resolved"
  }'
```

## Documentation

Swagger/OpenAPI is generated with Swashbuckle. XML documentation comments are enabled in the API project and included in Swagger, so public API descriptions appear in Swagger UI during development.

## Invalid Input Handling

The API uses ASP.NET Core model binding and validation to reject malformed request bodies with `400 Bad Request`. The request models explicitly validate required fields, string lengths, non-whitespace text values, enum values, and signal value ranges.

Examples that return `400 Bad Request`:

- `null` or missing required fields, for example a signal without `kind` or `value`
- numbers where strings or string enums are expected, for example `"name": 123` or `"kind": 1`
- invalid enum names, for example `"status": "NotARealStatus"`
- out-of-range signal values, for example `"value": 101`
- whitespace-only text values, for example `"name": "  "`

## Design Notes

- The API is deliberately small: servers, signal samples, and alarms.
- The in-memory repository keeps the prototype easy to run locally.
- `IMonitoringRepository` keeps storage replaceable, for example with SQLite later.
- JSON enum values are serialized as strings, so requests can use values such as `"Cpu"` or `"Resolved"`; numeric enum values are rejected.
- API integration tests cover routing, model validation, JSON options, dependency injection, controller behavior, and malformed input cases.
