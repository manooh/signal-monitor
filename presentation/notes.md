# Presentation Notes

## Demo Flow

1. Slides
2. Start the API with Docker Compose.
   ```bash
   docker compose up --build
   ```
3. Use the API with Swagger.
   - Open Swagger UI: `http://localhost:5050/swagger`
   - List all servers: `GET /api/servers`
   - Register a server: `POST /api/servers`
   - Open the created server by ID: `GET /api/servers/{id}`
   - Record a CPU or memory signal that raises an alarm: `POST /api/servers/{id}/signals`
   - Point out the `201 Created` response and copy the `Location` header.
   - Open the created signal with `GET /api/signals/{id}`.
   - List active alarms: `GET /api/alarms?status=Active`
   - Set the alarm to `Resolved`: `PUT /api/alarms/{id}/status`
   - Show one invalid request returning `400 Bad Request`: `POST /api/servers`
   - Optional: delete the demo server: `DELETE /api/servers/{id}`
4. Show the health check: `http://localhost:5050/health`.
5. Code walkthrough in VS Code.
6. Run or mention the checks:
   - `dotnet build signal-monitor.sln --no-restore -warnaserror`
   - `dotnet test signal-monitor.sln --no-build`
   - `dotnet list signal-monitor.sln package --vulnerable --include-transitive`
7. Stop the API.
   ```bash
   docker compose down
   ```

## Swagger Demo Entries

Create server: `POST /api/servers`

```json
{
  "name": "api-prod-02",
  "environment": "production",
  "region": "eu-central"
}
```

Get created server: `GET /api/servers/{id}`

```text
id = SERVER-ID-FROM-CREATE-RESPONSE
```

Record heartbeat, no alarm: `POST /api/servers/{id}/signals`

```json
{
  "kind": "Heartbeat",
  "value": 1,
  "unit": "ok"
}
```

Record CPU signal, no alarm: `POST /api/servers/{id}/signals`

```json
{
  "kind": "Cpu",
  "value": 42,
  "unit": "percent"
}
```

Record memory signal, no alarm: `POST /api/servers/{id}/signals`

```json
{
  "kind": "Memory",
  "value": 67,
  "unit": "percent"
}
```

Record CPU signal, warning alarm: `POST /api/servers/{id}/signals`

```json
{
  "kind": "Cpu",
  "value": 84,
  "unit": "percent"
}
```

After recording a signal, copy the `Location` response header.

Get created signal: `GET /api/signals/{id}`

```text
id = SIGNAL-ID-FROM-LOCATION-HEADER
```

Record memory signal, critical alarm: `POST /api/servers/{id}/signals`

```json
{
  "kind": "Memory",
  "value": 93,
  "unit": "percent"
}
```

Record failed heartbeat, critical alarm: `POST /api/servers/{id}/signals`

```json
{
  "kind": "Heartbeat",
  "value": 0,
  "unit": "ok"
}
```

List active alarms: `GET /api/alarms`

```text
status = Active
```

Resolve alarm: `PUT /api/alarms/{id}/status`

```json
{
  "status": "Resolved"
}
```

Invalid server request: `POST /api/servers`

```json
{
  "name": 123,
  "environment": "production",
  "region": "eu-central"
}
```

Invalid signal request: `POST /api/servers/{id}/signals`

```json
{
  "kind": 1,
  "value": 93
}
```

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
# http://localhost:5050/swagger
# https://localhost:5051/swagger
# http://localhost:5050/health
# https://localhost:5051/health
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
# http://localhost:5050/swagger
# http://localhost:5050/health
```

## API Commands

List servers:

```bash
curl http://localhost:5050/api/servers | jq
```

Filter servers:

```bash
curl "http://localhost:5050/api/servers?environment=production" | jq
curl "http://localhost:5050/api/servers?status=Warning" | jq
```

Create server:

```bash
curl -i -X POST http://localhost:5050/api/servers \
  -H "Content-Type: application/json" \
  -d '{
    "name": "api-prod-02",
    "environment": "production",
    "region": "eu-central"
  }'
```

Record high CPU:

```bash
curl -i -X POST http://localhost:5050/api/servers/YOUR-SERVER-ID-HERE/signals \
  -H "Content-Type: application/json" \
  -d '{
    "kind": "Cpu",
    "value": 93
  }'
```

List active alarms:

```bash
curl "http://localhost:5050/api/alarms?status=Active" | jq
```

Resolve alarm:

```bash
curl -i -X PUT http://localhost:5050/api/alarms/YOUR-ALARM-ID-HERE/status \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Resolved"
  }'
```

Invalid input examples:

```bash
# Wrong JSON type: name should be a non-empty string
curl -i -X POST http://localhost:5050/api/servers \
  -H "Content-Type: application/json" \
  -d '{
    "name": 123,
    "environment": "production",
    "region": "eu-central"
  }'

# Missing required signal kind
curl -i -X POST http://localhost:5050/api/servers/YOUR-SERVER-ID-HERE/signals \
  -H "Content-Type: application/json" \
  -d '{
    "value": 93
  }'

# Numeric enum values are rejected; use "Cpu", "Memory", or "Heartbeat"
curl -i -X POST http://localhost:5050/api/servers/YOUR-SERVER-ID-HERE/signals \
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
curl -i -X DELETE http://localhost:5050/api/servers/YOUR-SERVER-ID-HERE
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
lsof -i :5050
lsof -i :5051

# Kill a stuck local process after copying the PID from lsof
kill <PID>

# Check whether the API responds
curl -i http://localhost:5050/health
curl -k -i https://localhost:5051/health

# Check .NET environment and installed SDKs
dotnet --info

# Check repository status
git status --short

# Review local changes
git diff -- presentation/notes.md

# Check for whitespace problems before committing
git diff --check
```
