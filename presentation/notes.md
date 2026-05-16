# Presentation Notes

## Slides

URL:
```text
http://localhost:3030
```

Local:
```bash
cd presentation
npm install
npm run dev
```

Docker:
```bash
# start
docker compose -f presentation/docker-compose.yml up
# stop
docker compose -f presentation/docker-compose.yml down
# stop and reset
docker compose -f presentation/docker-compose.yml down -v
```

## Demo Flow

1. Slides
2. Run the tests.
3. Start the API with Docker Compose.
4. Open Swagger UI.
5. Register a server.
6. List/filter servers.
7. Record a CPU or memory signal that raises an alarm.
8. Resolve the alarm.
9. Show one invalid input example returning `400 Bad Request`.
10. Delete the server.
11. Show `/health`.
12. Briefly show the code structure in VS Code.

## .NET Commands

```bash
# Trust the local HTTPS development certificate, if needed
dotnet dev-certs https --trust

# Restore dependencies
dotnet restore signal-monitor.sln

# Build the solution
dotnet build signal-monitor.sln

# Run all tests
dotnet test signal-monitor.sln

# Start the API directly with .NET
dotnet run --project src/SignalMonitor.Api/SignalMonitor.Api.csproj

# Start the API with the HTTPS launch profile and file watching
dotnet watch run --launch-profile https --project src/SignalMonitor.Api/SignalMonitor.Api.csproj

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
docker compose logs -f signal-monitor-api

# Stop the API
docker compose down

# Check Swagger and health in the browser:
# http://localhost:5000/swagger
# http://localhost:5000/health
```

## API Commands

List servers:

```bash
curl http://localhost:5000/api/servers | jq
```

Filter servers:

```bash
curl "http://localhost:5000/api/servers?environment=production" | jq
curl "http://localhost:5000/api/servers?status=Warning" | jq
```

Create server:

```bash
curl -i -X POST http://localhost:5000/api/servers \
  -H "Content-Type: application/json" \
  -d '{
    "name": "api-prod-02",
    "environment": "production",
    "region": "eu-central"
  }'
```

Record high CPU:

```bash
curl -i -X POST http://localhost:5000/api/servers/YOUR-SERVER-ID-HERE/signals \
  -H "Content-Type: application/json" \
  -d '{
    "kind": "Cpu",
    "value": 93
  }'
```

List active alarms:

```bash
curl "http://localhost:5000/api/alarms?status=Active" | jq
```

Resolve alarm:

```bash
curl -i -X PUT http://localhost:5000/api/alarms/YOUR-ALARM-ID-HERE/status \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Resolved"
  }'
```

Invalid input examples:

```bash
# Wrong JSON type: name should be a non-empty string
curl -i -X POST http://localhost:5000/api/servers \
  -H "Content-Type: application/json" \
  -d '{
    "name": 123,
    "environment": "production",
    "region": "eu-central"
  }'

# Missing required signal kind
curl -i -X POST http://localhost:5000/api/servers/YOUR-SERVER-ID-HERE/signals \
  -H "Content-Type: application/json" \
  -d '{
    "value": 93
  }'

# Numeric enum values are rejected; use "Cpu", "Memory", or "Heartbeat"
curl -i -X POST http://localhost:5000/api/servers/YOUR-SERVER-ID-HERE/signals \
  -H "Content-Type: application/json" \
  -d '{
    "kind": 1,
    "value": 93
  }'
```

Production considerations to mention if asked:

- DDoS protection is intentionally outside this demo. In production, use rate limiting and request size limits in the API, plus reverse proxy, API gateway, or cloud edge protection in front of it.
- Authorization would be the first real security addition. Protect write operations, especially creating/deleting servers, ingesting signals, and resolving alarms.
- Access control could be role-based: read-only viewer, signal writer, operator for alarm status changes, and admin for server management.
- If the signal table grew to 100 million rows, avoid raw unbounded list endpoints. Use indexed persistent storage, mandatory date ranges, cursor pagination, retention/archive policies, and precomputed dashboard aggregates.
- Parallelism is deliberately simple here. The in-memory repository uses a lock for consistent demo state; production would use database concurrency controls, async I/O, pagination, queues, and background workers for higher signal volume.

Delete server:

```bash
curl -i -X DELETE http://localhost:5000/api/servers/YOUR-SERVER-ID-HERE
```

## Quick Recovery

```bash
# Check running containers
docker ps

# Check Docker Compose service status
docker compose ps

# Follow API logs
docker compose logs -f signal-monitor-api

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
