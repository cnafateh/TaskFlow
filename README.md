# TaskFlow

TaskFlow is a server-rendered task and project management application built with ASP.NET Core MVC, Entity Framework Core, ASP.NET Core Identity, SQLite, and Docker.

The project was developed as a practical learning project with an emphasis on clean application structure, ownership rules, administration workflows, authentication, containerized deployment, and production-oriented development habits.

## Features

- User registration, login, logout, and account management
- Per-user project ownership
- Per-user category ownership
- Task management with project and category assignment
- Task priority and status tracking
- Search, filtering, sorting, and pagination
- Role-based authorization with `Admin` and `User` roles
- Administrative dashboard
- Admin user management and role changes
- Admin task management across all users
- Bulk task status updates and bulk deletion
- Admin project and category management
- Custom access denied, 404, and error pages
- Persistent SQLite storage
- Persistent ASP.NET Core Data Protection keys
- Docker deployment
- GitHub Actions image publishing to GitHub Container Registry

## Technology Stack

- .NET 10
- ASP.NET Core MVC
- Razor Views
- ASP.NET Core Identity
- Entity Framework Core
- SQLite
- Bootstrap
- Custom CSS
- Docker / Docker Compose
- GitHub Actions
- GitHub Container Registry

## Architecture

TaskFlow follows a service-oriented MVC structure:

```text
Request
  ↓
Controller
  ↓
Service
  ↓
Entity Framework Core / AppDbContext
  ↓
SQLite
```

Controllers handle HTTP concerns and delegate application logic to services. User-facing operations enforce ownership rules before reading or changing data. Administrative operations are isolated through `AdminService`.

See [Architecture](docs/ARCHITECTURE.md) for more detail.

## Application Flow

A normal user can:

```text
Register / Login
      ↓
Create Project
      ↓
Create Category
      ↓
Create Task
      ↓
Search / Filter / Edit / Track
```

An administrator can additionally:

```text
Admin Dashboard
      ↓
Users / Tasks / Projects / Categories
      ↓
Filter / Review / Change Roles / Bulk Actions
```

See [Application Flow](docs/APPLICATION_FLOW.md).

## Local Development

### Requirements

- .NET 10 SDK
- Git
- Docker Desktop, optional for container testing

### Run directly with .NET

```bash
dotnet restore
dotnet build
dotnet run --project TaskFlow.Web
```

### Development administrator secrets

TaskFlow uses .NET User Secrets for local bootstrap credentials:

```bash
dotnet user-secrets set "SeedAdmin:Email" "admin@example.com" --project TaskFlow.Web
dotnet user-secrets set "SeedAdmin:Password" "your-strong-password" --project TaskFlow.Web
```

Secrets are not stored in the repository.

## Docker

Create a local environment file:

```bash
cp .env.example .env
```

Set real values:

```env
SEED_ADMIN_EMAIL=admin@example.com
SEED_ADMIN_PASSWORD=replace-with-a-strong-password
```

Build and run:

```bash
docker compose build
docker compose up -d
```

Application:

```text
http://localhost:8080
```

Health endpoint:

```text
http://localhost:8080/health
```

The Docker deployment persists:

- SQLite database in `taskflow_data`
- Data Protection keys in `taskflow_keys`

Do not run `docker compose down -v` unless you intentionally want to remove persistent application data.

## Container Registry

Every push to `main` triggers:

```text
.github/workflows/docker-publish.yml
```

The workflow builds the Docker image and publishes it to GitHub Container Registry:

```text
ghcr.io/<github-owner>/<repository>:latest
```

A commit-specific `sha-*` tag is also published.

## Production Configuration

TaskFlow reads configuration through ASP.NET Core `IConfiguration`.

Typical production environment variables:

```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Data Source=/app/data/taskflow.db
DataProtection__KeysPath=/app/keys
SeedAdmin__Email=admin@example.com
SeedAdmin__Password=strong-bootstrap-password
```

For deployment details, see [Deployment](docs/DEPLOYMENT.md).

## Security Notes

The application currently includes:

- Role-based authorization
- Server-side ownership enforcement
- Anti-forgery validation on destructive operations
- Admin self-role-change protection
- Last-admin protection
- Configuration-based bootstrap credentials
- Non-root Docker runtime
- Persistent Data Protection keys

See [Security](docs/SECURITY.md).

## Documentation

- [Architecture](docs/ARCHITECTURE.md)
- [Application Flow](docs/APPLICATION_FLOW.md)
- [Deployment](docs/DEPLOYMENT.md)
- [Security](docs/SECURITY.md)

## Project Status

TaskFlow is actively evolving. The current version is container-ready and suitable for iterative deployment.

Planned improvements include:

- Automated tests
- Additional security hardening
- Improved logging and observability
- Further admin workflow refinement
- Production monitoring

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE).
