# TaskFlow

TaskFlow is a task and project management application built with ASP.NET Core MVC, Entity Framework Core, ASP.NET Core Identity, and Docker.

The project was developed as a practical software engineering project with an emphasis on application architecture, authentication and authorization, per-user data ownership, automated testing, containerized deployment, and production-oriented development practices.

TaskFlow uses SQLite for lightweight local development and PostgreSQL in production.

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
- Ownership enforcement for user resources
- Custom access denied, 404, and error pages
- Structured application logging
- Request trace identifiers for error correlation
- Health check endpoint
- Automated integration tests
- SQLite development database
- PostgreSQL production database
- Persistent ASP.NET Core Data Protection keys
- Docker deployment
- GitHub Actions CI
- Automated Docker image publishing to GitHub Container Registry

## Technology Stack

- .NET 10
- C#
- ASP.NET Core MVC
- Razor Views
- ASP.NET Core Identity
- Entity Framework Core
- SQLite
- PostgreSQL
- Npgsql
- xUnit
- Bootstrap
- Custom CSS
- Docker / Docker Compose
- GitHub Actions
- GitHub Container Registry
- Nginx Proxy Manager

## Architecture

TaskFlow currently follows a service-oriented MVC architecture:

```text
HTTP Request
     ↓
Controller
     ↓
Service Interface
     ↓
Service Implementation
     ↓
Entity Framework Core / AppDbContext
     ↓
Database
```

Controllers handle HTTP and presentation concerns while application operations are delegated to services.

User-facing services enforce ownership rules before accessing or modifying data. Administrative cross-user operations are isolated through `AdminService`.

The application currently uses the same domain and service model for its MVC application. The architecture is being prepared for a separate REST API while keeping business rules shared between both entry points.

See [Architecture](docs/ARCHITECTURE.md) for more detail.

## Database Strategy

TaskFlow uses different database providers depending on the environment.

### Development

```text
ASP.NET Core
     ↓
Entity Framework Core
     ↓
SQLite
```

SQLite provides a lightweight local development environment without requiring a database server.

The development database is created from the current EF Core model.

### Production

```text
ASP.NET Core
     ↓
Entity Framework Core
     ↓
Npgsql
     ↓
PostgreSQL
```

Production uses PostgreSQL with EF Core migrations.

The application applies pending migrations during startup.

This allows development to remain lightweight while production uses a dedicated relational database server.

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

## Authentication and Authorization

TaskFlow uses ASP.NET Core Identity.

Available roles:

```text
Admin
User
```

Authentication determines who the current user is.

Authorization and ownership rules determine which resources that user may access.

For normal users:

```text
Project.UserId == CurrentUserId
```

```text
Category.UserId == CurrentUserId
```

Task ownership is derived from the owning Project:

```text
Task.Project.UserId == CurrentUserId
```

Administrative operations use a separate privileged service layer for cross-user management.

## Local Development

### Requirements

- .NET 10 SDK
- Git

Docker Desktop is optional for normal local development.

### Restore

```bash
dotnet restore
```

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run --project TaskFlow.Web
```

The Development environment uses SQLite automatically.

The local connection string is configured through:

```text
appsettings.Development.json
```

with a connection similar to:

```text
Data Source=taskflow.db
```

## Development Administrator

TaskFlow uses .NET User Secrets for local bootstrap administrator credentials.

Set them with:

```bash
dotnet user-secrets set "SeedAdmin:Email" "admin@example.com" --project TaskFlow.Web
```

```bash
dotnet user-secrets set "SeedAdmin:Password" "your-strong-password" --project TaskFlow.Web
```

User Secrets are stored outside the repository and should not be committed to source control.

## Automated Tests

TaskFlow includes an independent xUnit test project:

```text
TaskFlow.Tests
```

The current automated test suite covers important service behavior including:

- Per-user task ownership
- Preventing access to another user's tasks
- Task creation ownership validation
- Task update ownership validation
- Task deletion ownership validation
- Task filtering
- Task summary behavior
- Administrative role-management rules
- Protection against changing the current administrator's own role
- Protection against removing the last administrator
- Administrative bulk task deletion
- Category deletion rules

The service integration tests use an isolated SQLite in-memory database.

This allows each test to run against a real relational database implementation without modifying development or production data.

Run the test suite with:

```bash
dotnet test
```

The current test suite contains 16 automated tests.

## CI

Pull requests targeting `main` are validated using GitHub Actions.

The CI pipeline performs:

```text
Restore
   ↓
Build
   ↓
Automated Tests
```

Changes should pass both compilation and automated tests before being merged into `main`.

## Docker

TaskFlow is distributed as a Docker image.

The production application container:

- Runs ASP.NET Core
- Connects to PostgreSQL
- Stores Data Protection keys in a persistent Docker volume
- Runs behind a reverse proxy
- Does not store application data inside the application container

The production container architecture is:

```text
Internet
   ↓
Nginx Proxy Manager
   ↓
proxy Docker network
   ↓
TaskFlow
   ↓
database Docker network
   ↓
PostgreSQL
```

The application and PostgreSQL containers communicate through an internal Docker network.

PostgreSQL does not need to be exposed directly to the public internet.

## Health Check

TaskFlow exposes a health endpoint:

```text
/health
```

For example:

```text
https://your-domain.example/health
```

This endpoint can be used by deployment and monitoring systems to verify that the application is running.

## Container Registry

Every push to `main` triggers the Docker publishing workflow:

```text
.github/workflows/docker-publish.yml
```

The workflow builds the application image and publishes it to GitHub Container Registry.

The primary image is:

```text
ghcr.io/cnafateh/taskflow:latest
```

Commit-specific image tags are also generated.

## Production Configuration

Production configuration is supplied using environment variables rather than storing secrets in source control.

Typical production configuration includes:

```env
ASPNETCORE_ENVIRONMENT=Production

ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=taskflow;Username=taskflow_app;Password=<database-password>

DataProtection__KeysPath=/app/keys

SeedAdmin__Email=admin@example.com
SeedAdmin__Password=<strong-bootstrap-password>
```

When Docker Compose variable substitution is used, the database password can instead be supplied through:

```env
TASKFLOW_DB_PASSWORD=<database-password>
```

Secrets must not be committed to the repository.

For deployment details, see [Deployment](docs/DEPLOYMENT.md).

## Production Database

The production deployment uses PostgreSQL.

A dedicated database and application role are used:

```text
Database
└── taskflow

Application Role
└── taskflow_app
```

The application account is separate from the PostgreSQL administrative account.

EF Core migrations are used to manage the production schema.

## Logging and Error Handling

TaskFlow uses ASP.NET Core structured logging.

Application actions include information such as:

- Controller
- Action
- User identifier
- Execution duration
- HTTP status
- Request trace identifier

Unexpected production errors are handled through the global ASP.NET Core exception handler.

Users receive a generic error page while technical information remains in application logs.

Sensitive values such as passwords, cookies, authentication tokens, and secrets are not intentionally logged.

## Security

TaskFlow currently includes:

- ASP.NET Core Identity authentication
- Role-based authorization
- Server-side ownership enforcement
- Anti-forgery validation for destructive MVC operations
- Admin self-role-change protection
- Last-administrator protection
- Configuration-based bootstrap credentials
- .NET User Secrets for development credentials
- Environment-based production secrets
- Non-root Docker runtime
- Persistent Data Protection keys
- Database user isolation
- Internal Docker database networking

See [Security](docs/SECURITY.md).

## Repository Structure

The current solution contains:

```text
TaskFlow
│
├── TaskFlow.Web
│   ├── Controllers
│   ├── Data
│   ├── Filters
│   ├── Models
│   ├── Services
│   ├── ViewModels
│   └── Views
│
├── TaskFlow.Tests
│   ├── Infrastructure
│   └── Services
│
├── docs
│
├── Dockerfile
├── docker-compose.yml
└── TaskFlow.slnx
```

The architecture will evolve as the REST API is introduced so that reusable application and persistence concerns can be shared between the MVC and API entry points.

## Documentation

Additional documentation is available in:

- [Architecture](docs/ARCHITECTURE.md)
- [Application Flow](docs/APPLICATION_FLOW.md)
- [Deployment](docs/DEPLOYMENT.md)
- [Security](docs/SECURITY.md)

## Project Status

The MVC application is feature-complete for the current project scope and is deployed using Docker with PostgreSQL in production.

Current capabilities include:

```text
MVC Application              ✅
Authentication               ✅
Authorization                ✅
Per-user ownership           ✅
Administrative workflows     ✅
SQLite local development     ✅
PostgreSQL production        ✅
Automated tests              ✅
CI                           ✅
Docker deployment            ✅
GHCR image publishing        ✅
Production health endpoint   ✅
```

The next major development phase is the introduction of a REST API.

Planned work includes:

- Refactoring shared application concerns out of the MVC project
- Introducing clearer dependency boundaries
- Adding a dedicated `TaskFlow.Api` project
- RESTful endpoints
- API DTOs
- API validation
- HTTP status-code handling
- Bearer/JWT authentication
- API authorization and ownership enforcement
- OpenAPI documentation
- API integration tests
- Independent API deployment

## License

This project is licensed under the MIT License.

See [LICENSE](LICENSE).