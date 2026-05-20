# Signal Monitor

A small .NET 8 REST API for monitoring server signals.

The app keeps the data in memory, exposes Swagger/OpenAPI in development, and can be started locally or with Docker. It is intentionally compact: register servers, record heartbeat/CPU/memory signals, and show simple alarms when signal values cross a threshold.

## Requirements

- Local run: .NET 8 SDK
- Docker run: Docker Desktop or Docker Engine with Compose

## Run

### Run Locally

```bash
# Start the API
dotnet run --project src/SignalMonitor.Api/SignalMonitor.Api.csproj

# Or start it with the HTTPS launch profile
dotnet run --project src/SignalMonitor.Api/SignalMonitor.Api.csproj --launch-profile https
```

### Run with Docker

```bash
# Build and start the container
docker compose up --build

# Stop and remove the container
docker compose down

# Manual image build/run, if needed
docker build -t signal-monitor-api .
docker run --rm -p 5050:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  --name signal-monitor-api \
  signal-monitor-api
```

## Default URLs

- `http://localhost:5050`
- `https://localhost:5051` when using the HTTPS launch profile
- Swagger: `http://localhost:5050/swagger`

## Build and Test

```bash
dotnet build signal-monitor.sln
dotnet test signal-monitor.sln
```

## API

| Method | Path | Purpose |
| --- | --- | --- |
| `GET` | `/api/servers` | List servers, optionally filtered by `environment` or `status` |
| `GET` | `/api/servers/{id}` | Get one server |
| `POST` | `/api/servers` | Register a server |
| `POST` | `/api/servers/{id}/signals` | Record a heartbeat, CPU, or memory signal |
| `DELETE` | `/api/servers/{id}` | Delete a server and its related data |
| `GET` | `/api/signals` | List signal samples, optionally filtered by `serverId` or `kind` |
| `GET` | `/api/signals/{id}` | Get one signal sample |
| `GET` | `/api/alarms` | List alarms, optionally filtered by `serverId` or `status` |
| `PUT` | `/api/alarms/{id}/status` | Update an alarm status |
| `GET` | `/health` | Health check |

## Example Requests

List servers:

```bash
curl http://localhost:5050/api/servers
```

Register a server:

```bash
curl -i -X POST http://localhost:5050/api/servers \
  -H "Content-Type: application/json" \
  -d '{
    "name": "api-prod-02",
    "environment": "production",
    "region": "eu-central"
  }'
```

Record a CPU signal:

```bash
curl -i -X POST http://localhost:5050/api/servers/YOUR-SERVER-ID-HERE/signals \
  -H "Content-Type: application/json" \
  -d '{
    "kind": "Cpu",
    "value": 93
  }'
```

Resolve an alarm:

```bash
curl -i -X PUT http://localhost:5050/api/alarms/YOUR-ALARM-ID-HERE/status \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Resolved"
  }'
```

## Notes

- Data is stored in memory and seeded from `appsettings.json`.
- `IMonitoringRepository` keeps the storage layer replaceable, for example with SQLite later.
- JSON enums are serialized as strings, so requests use values like `"Cpu"` or `"Resolved"`.
- Invalid input returns `400 Bad Request`, including missing required fields, wrong JSON types, invalid enum values, and out-of-range signal values.
- Swagger is generated with Swashbuckle and includes XML documentation comments.

## Scope

This project keeps the runtime small on purpose. For a production version, the main changes would be persistent storage, authentication and roles, pagination for larger signal volumes, configurable thresholds, and operational telemetry.
