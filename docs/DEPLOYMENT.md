# TaskFlow Deployment

## Deployment Model

TaskFlow is designed to run as a Docker container while keeping persistent state outside the disposable container.

```text
taskflow_data
    → SQLite database

taskflow_keys
    → ASP.NET Core Data Protection keys
```

## Build

From the repository root:

```bash
docker compose build
```

The Docker build uses a configurable NuGet source. The current default is:

```text
https://package-mirror.liara.ir/repository/nuget/index.json
```

It can be overridden using the `NUGET_SOURCE` Compose build argument.

## Environment File

Copy:

```bash
cp .env.example .env
```

Configure real values:

```env
NUGET_SOURCE=https://package-mirror.liara.ir/repository/nuget/index.json
SEED_ADMIN_EMAIL=admin@example.com
SEED_ADMIN_PASSWORD=replace-with-a-strong-password
```

Never commit `.env`.

## Run

```bash
docker compose up -d
```

Status:

```bash
docker compose ps
```

Logs:

```bash
docker compose logs -f taskflow
```

## Health Endpoint

```text
/health
```

Example:

```bash
curl http://localhost:8080/health
```

Expected:

```text
Healthy
```

## Database Initialization

TaskFlow applies existing EF Core migrations on application startup.

```text
Empty persistent volume
      ↓
Application startup
      ↓
Migrations
      ↓
SQLite schema created
      ↓
Identity roles seeded
      ↓
Bootstrap admin created
```

## Persistent Volumes

```text
taskflow_data:/app/data
taskflow_keys:/app/keys
```

Do not run:

```bash
docker compose down -v
```

unless removal of persistent data is intentional.

## GitHub Container Registry

The repository includes:

```text
.github/workflows/docker-publish.yml
```

Each push to `main` publishes:

```text
ghcr.io/<owner>/<repository>:latest
```

and a commit-specific:

```text
ghcr.io/<owner>/<repository>:sha-...
```

The workflow authenticates to GHCR with GitHub's built-in `GITHUB_TOKEN`.

## Arcane Deployment

Arcane should pull:

```text
ghcr.io/<owner>/<repository>:latest
```

Recommended production environment variables:

```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Data Source=/app/data/taskflow.db
DataProtection__KeysPath=/app/keys
SeedAdmin__Email=admin@example.com
SeedAdmin__Password=strong-bootstrap-password
```

Persistent mounts:

```text
/app/data
/app/keys
```

Container port:

```text
8080
```

Typical public topology:

```text
Internet
   ↓
HTTPS reverse proxy
   ↓
taskflow.example.com
   ↓
TaskFlow container :8080
```

## Update Flow

```text
Push to main
      ↓
GitHub Actions
      ↓
New GHCR image
      ↓
Arcane pulls/recreates container
      ↓
Existing volumes reused
      ↓
Migrations run
      ↓
Updated version online
```

## Production Checklist

- use a strong bootstrap administrator password
- keep `.env` out of Git
- verify database persistence
- verify Data Protection key persistence
- publish only through the reverse proxy
- configure HTTPS
- verify `/health`
- test login and logout
- test persistence across container recreation
- back up the SQLite volume
