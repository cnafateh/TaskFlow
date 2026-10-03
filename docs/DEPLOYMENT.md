# TaskFlow Deployment

## 1. Production Model

TaskFlow runs as a Dockerized ASP.NET Core application.

Production persistence uses PostgreSQL.

```text
Internet
   ↓
HTTPS
   ↓
Nginx Proxy Manager
   ↓
proxy Docker network
   ↓
TaskFlow container
   ↓
database Docker network
   ↓
PostgreSQL container
```

The application container is disposable.

PostgreSQL data lives independently from the TaskFlow application container.

---

## 2. Docker Networks

TaskFlow participates in:

```text
proxy
database
```

The `proxy` network connects TaskFlow to Nginx Proxy Manager.

The `database` network connects TaskFlow to PostgreSQL.

The database does not need to be exposed to the public internet.

---

## 3. Production Database

Production uses:

```text
PostgreSQL
```

Typical logical configuration:

```text
Database:
taskflow

Application role:
taskflow_app
```

The application should use a dedicated PostgreSQL role rather than an administrative database account.

---

## 4. Production Connection String

Example:

```env
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=taskflow;Username=taskflow_app;Password=<database-password>
```

The actual password must be supplied through the deployment environment.

---

## 5. Environment Variables

Typical production configuration:

```env
ASPNETCORE_ENVIRONMENT=Production

ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

TASKFLOW_DB_PASSWORD=<database-password>

DataProtection__KeysPath=/app/keys

SeedAdmin__Email=admin@example.com
SeedAdmin__Password=<strong-bootstrap-password>
```

Secrets must not be committed to Git.

---

## 6. Docker Compose

The TaskFlow service is expected to:

- run the published application image
- expose port `8080` internally
- connect to the `proxy` network
- connect to the `database` network
- receive production configuration through environment variables
- persist Data Protection keys

---

## 7. Reverse Proxy

TaskFlow listens internally on:

```text
8080
```

Nginx Proxy Manager can forward traffic to:

```text
http://taskflow:8080
```

Because the reverse proxy shares the Docker `proxy` network, the TaskFlow container does not require a public host port.

---

## 8. Data Protection

ASP.NET Core authentication cookies depend on Data Protection keys.

Production persists these keys outside the disposable application container.

Application path:

```text
/app/keys
```

Without persistent Data Protection keys, authentication cookies can become invalid after container recreation.

---

## 9. Database Provider Strategy

Development:

```text
ASP.NET Core
   ↓
EF Core
   ↓
SQLite
```

Production:

```text
ASP.NET Core
   ↓
EF Core
   ↓
Npgsql
   ↓
PostgreSQL
```

Provider selection occurs during application startup.

---

## 10. EF Core Migrations

Production database schema is managed through EF Core migrations.

Migrations live in:

```text
TaskFlow.Infrastructure/Persistence/Migrations
```

Production startup applies pending migrations.

Development may use a disposable SQLite database created from the current model.

---

## 11. Docker Image

The production application image is published to GitHub Container Registry.

Primary image:

```text
ghcr.io/cnafateh/taskflow:latest
```

Commit-specific tags may also be produced.

---

## 12. CI Flow

Pull requests targeting `main` run:

```text
Checkout
   ↓
Restore
   ↓
Build
   ↓
Tests
```

Only code that passes the required checks should be merged.

---

## 13. Deployment Flow

```text
Feature Branch
      ↓
Pull Request
      ↓
CI
      ↓
Merge to main
      ↓
Docker image build
      ↓
GHCR
      ↓
deployment platform pulls image
      ↓
TaskFlow container recreated
```

---

## 14. Container Recreation

When the application container is recreated:

```text
New TaskFlow container
      ↓
existing Data Protection volume mounted
      ↓
existing PostgreSQL database reused
      ↓
pending migrations applied
      ↓
application starts
```

Application data is not stored in the disposable TaskFlow container.

---

## 15. Health Check

TaskFlow exposes:

```text
/health
```

Example:

```bash
curl https://your-domain.example/health
```

The endpoint can be used by deployment and monitoring systems.

---

## 16. Build

From the repository root:

```bash
docker compose build
```

The Docker build may use a configurable NuGet source.

Current mirror:

```text
https://package-mirror.liara.ir/repository/nuget/index.json
```

---

## 17. Run

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

---

## 18. Update

Typical update process:

```text
New GHCR image
      ↓
deployment pulls image
      ↓
old TaskFlow container replaced
      ↓
PostgreSQL stays running
      ↓
Data Protection keys remain persistent
      ↓
application starts
```

---

## 19. Production Checklist

Before deployment verify:

- CI is green
- PostgreSQL is available
- dedicated application database role exists
- database password is stored outside Git
- Data Protection keys are persistent
- HTTPS is enabled
- reverse proxy routing is correct
- forwarded headers are enabled
- `/health` responds
- login works
- login survives container recreation
- migrations apply successfully
- database backups exist