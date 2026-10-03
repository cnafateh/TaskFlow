# TaskFlow

TaskFlow is a task and project management application built with ASP.NET Core, Entity Framework Core, ASP.NET Core Identity, PostgreSQL, SQLite, Docker, and automated testing.

The project is developed as a practical software engineering project with a strong focus on architecture, dependency management, authentication and authorization, ownership enforcement, automated testing, and production-oriented deployment.

The MVC application is complete for the current scope. Its shared application and persistence layers have been separated in preparation for introducing a dedicated REST API without duplicating business logic.

## Features

- User registration, login, logout, and account management
- ASP.NET Core Identity
- `Admin` and `User` roles
- Per-user Projects
- Per-user Categories
- Task creation, editing, deletion, and details
- Task priority and status tracking
- Search, filtering, sorting, and pagination
- Task summary statistics
- Administrative dashboard
- Admin user and role management
- Admin task management across all users
- Bulk task actions
- Admin project and category management
- Server-side ownership enforcement
- Custom Access Denied, 404, and error pages
- Structured request logging
- Request trace identifiers
- Health check endpoint
- Automated tests
- SQLite for local development
- PostgreSQL for production
- EF Core migrations
- Docker deployment
- Persistent ASP.NET Core Data Protection keys
- GitHub Actions CI
- GitHub Container Registry publishing

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
- Docker
- Docker Compose
- GitHub Actions
- GitHub Container Registry
- Nginx Proxy Manager

# Architecture

TaskFlow uses separate projects for Domain, Application, Infrastructure, and MVC presentation concerns.

```text
TaskFlow.Domain
TaskFlow.Application
TaskFlow.Infrastructure
TaskFlow.Web
TaskFlow.Tests
```

The important dependency rule is:

```text
                    Domain
                      ▲
                      │
                 Application
                  ▲        ▲
                  │        │
                Web   Infrastructure
```

The arrows represent source-code dependencies.

Inner application layers do not depend on outer technical implementation details.

## Domain

`TaskFlow.Domain` contains the core business concepts.

Current examples:

```text
TaskItem
Project
Category
TaskPriority
TaskStatus
```

Domain does not depend on:

- ASP.NET Core MVC
- Entity Framework Core
- ASP.NET Core Identity
- PostgreSQL
- SQLite
- Infrastructure
- Web

## Application

`TaskFlow.Application` contains reusable application behavior and contracts.

Examples:

```text
ITaskService
TaskService

IProjectService
ProjectService

ICategoryService
CategoryService
```

Persistence contracts also belong to Application:

```text
ITaskRepository
IProjectRepository
ICategoryRepository
```

Application additionally contains reusable models such as:

```text
TaskFilter
TaskSummary
PagedResult<T>
```

Application depends on Domain but does not depend on Infrastructure, EF Core, or MVC.

## Infrastructure

`TaskFlow.Infrastructure` contains technical implementation details.

```text
TaskFlow.Infrastructure
│
├── Identity
│   ├── ApplicationUser
│   └── IdentitySeeder
│
└── Persistence
    ├── AppDbContext
    ├── Migrations
    └── Repositories
        ├── TaskRepository
        ├── ProjectRepository
        └── CategoryRepository
```

Infrastructure implements repository contracts defined by Application.

Example:

```text
Application
└── ITaskRepository
        ▲
        │ implements
Infrastructure
└── TaskRepository
```

`TaskRepository` may use EF Core and `AppDbContext`.

`TaskService` does not.

## Web

`TaskFlow.Web` is the MVC presentation layer and the current application entry point.

It contains:

- Controllers
- Razor Views
- MVC ViewModels
- Filters
- HTTP-specific behavior
- TempData
- request pipeline configuration
- dependency injection registration
- startup configuration

`Program.cs` acts as the MVC composition root.

The reusable Task, Project, and Category services no longer live in the Web project.

## Request Flow

The main reusable request flow is now:

```text
Browser
   ↓
MVC Controller
   ↓
Application Service
   ↓
Repository Interface
   ↓
Infrastructure Repository
   ↓
AppDbContext
   ↓
EF Core
   ↓
Database
```

For example:

```text
TasksController
   ↓
ITaskService
   ↓
TaskService
   ↓
ITaskRepository
   ↓
TaskRepository
   ↓
AppDbContext
```

`TaskService` can also use:

```text
IProjectRepository
ICategoryRepository
```

when a Task use case needs Project or Category ownership validation.

## Dependency Inversion

Application defines the persistence capability it requires:

```csharp
public interface ITaskRepository
{
    // persistence operations required by Task use cases
}
```

Infrastructure implements that contract:

```csharp
public class TaskRepository : ITaskRepository
{
    // EF Core implementation
}
```

This means Application does not need to reference Infrastructure.

The ASP.NET Core DI container connects abstractions to concrete implementations at runtime.

## MVC and Future API

The architecture is now prepared for another presentation layer:

```text
              MVC Controllers
                    │
                    ▼
               Application
                    ▲
                    │
               API Controllers
```

The future API will not duplicate Task, Project, or Category business/application logic.

Both MVC and API entry points will use the same Application layer.

See:

- [Architecture](docs/ARCHITECTURE.md)
- [Dependency Flow](docs/DEPENDENCY_FLOW.md)
- [Application Flow](docs/APPLICATION_FLOW.md)

# Database Strategy

TaskFlow uses different database providers depending on environment.

## Development

```text
Application
   ↓
EF Core
   ↓
SQLite
```

SQLite keeps local development lightweight.

## Production

```text
Application
   ↓
EF Core
   ↓
Npgsql
   ↓
PostgreSQL
```

Production schema changes are managed through EF Core migrations.

Migrations live in:

```text
TaskFlow.Infrastructure/Persistence/Migrations
```

Pending migrations are applied during production startup.

# Authentication and Authorization

TaskFlow uses ASP.NET Core Identity.

Roles:

```text
Admin
User
```

Authentication determines who the current user is.

Authorization determines which functionality the user may access.

Normal application resources additionally enforce ownership.

Project ownership:

```text
Project.UserId == CurrentUserId
```

Category ownership:

```text
Category.UserId == CurrentUserId
```

Task ownership is derived through the Project:

```text
Task.Project.UserId == CurrentUserId
```

Ownership is enforced server-side.

# Local Development

## Requirements

- .NET 10 SDK
- Git

## Restore

```bash
dotnet restore ./TaskFlow.slnx
```

When the official NuGet source is unavailable:

```bash
dotnet restore ./TaskFlow.slnx \
  --source https://package-mirror.liara.ir/repository/nuget/index.json \
  --disable-parallel
```

## Build

```bash
dotnet build ./TaskFlow.slnx
```

## Test

```bash
dotnet test ./TaskFlow.Tests/TaskFlow.Tests.csproj
```

## Run

```bash
dotnet run --project TaskFlow.Web
```

Local development uses SQLite.

# Development Administrator

Development bootstrap administrator credentials use .NET User Secrets.

```bash
dotnet user-secrets set "SeedAdmin:Email" "admin@example.com" --project TaskFlow.Web
```

```bash
dotnet user-secrets set "SeedAdmin:Password" "your-strong-password" --project TaskFlow.Web
```

`Program.cs` reads the configuration values and passes the required values to `IdentitySeeder`.

The Infrastructure seeder does not need to know how configuration is stored.

# Automated Tests

Automated tests live in:

```text
TaskFlow.Tests
```

The current suite contains 16 automated tests.

Current coverage includes:

- per-user Task ownership
- preventing access to another user's Tasks
- Task creation ownership validation
- Task update ownership validation
- Task deletion ownership validation
- Task filtering
- Task summary behavior
- administrator role safeguards
- prevention of administrator self-demotion
- last-administrator protection
- bulk administrative Task deletion
- Category deletion behavior

Service integration tests use an isolated SQLite in-memory database.

The test environment exercises real EF Core relational behavior without modifying development or production data.

# CI

Pull requests targeting `main` are validated using GitHub Actions.

```text
Checkout
   ↓
Restore
   ↓
Build
   ↓
Automated Tests
```

Changes are expected to pass compilation and tests before merge.

# Docker and Production

Production topology:

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
PostgreSQL
```

The TaskFlow application container is disposable.

Database data is owned by PostgreSQL.

ASP.NET Core Data Protection keys are stored in a persistent Docker volume.

# Health Check

TaskFlow exposes:

```text
/health
```

This can be used by deployment and monitoring systems.

# Container Registry

Pushes to `main` can publish the application image to GitHub Container Registry.

Primary image:

```text
ghcr.io/cnafateh/taskflow:latest
```

# Production Configuration

Typical production configuration:

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=taskflow;Username=taskflow_app;Password=<database-password>

DataProtection__KeysPath=/app/keys

SeedAdmin__Email=admin@example.com
SeedAdmin__Password=<strong-bootstrap-password>
```

Secrets must not be committed to source control.

# Repository Structure

```text
TaskFlow
│
├── TaskFlow.Domain
│   ├── Entities
│   └── Enums
│
├── TaskFlow.Application
│   ├── Common
│   ├── Tasks
│   ├── Projects
│   └── Categories
│
├── TaskFlow.Infrastructure
│   ├── Identity
│   └── Persistence
│       ├── Migrations
│       └── Repositories
│
├── TaskFlow.Web
│   ├── Controllers
│   ├── Filters
│   ├── Services
│   ├── ViewModels
│   ├── Views
│   └── Program.cs
│
├── TaskFlow.Tests
│
├── docs
│
├── Dockerfile
├── docker-compose.yml
└── TaskFlow.slnx
```

`TaskFlow.Web/Services` still contains MVC-specific services such as administrative functionality where extraction would not currently provide a useful shared application boundary.

# Architecture Status

```text
MVC application                        ✅
Authentication                         ✅
Authorization                          ✅
Per-user ownership                     ✅
Administrative workflows               ✅
Automated tests                        ✅
SQLite development                     ✅
PostgreSQL production                  ✅
Docker deployment                      ✅
CI                                     ✅

Domain project                         ✅
Application project                    ✅
Infrastructure project                 ✅
Domain entities extracted              ✅
Application service contracts          ✅
Application service implementations    ✅
Repository contracts                   ✅
Repository implementations             ✅
AppDbContext in Infrastructure         ✅
Identity in Infrastructure             ✅
EF migrations in Infrastructure        ✅
Application independent of EF Core     ✅
Application independent of Web         ✅
Application independent of Infrastructure ✅

REST API                               ⏳
```

The next major phase is the introduction of `TaskFlow.Api`.

# Documentation

- [Architecture](docs/ARCHITECTURE.md)
- [Dependency Flow](docs/DEPENDENCY_FLOW.md)
- [Application Flow](docs/APPLICATION_FLOW.md)
- [Deployment](docs/DEPLOYMENT.md)
- [Security](docs/SECURITY.md)

# License

TaskFlow is licensed under the MIT License.

See [LICENSE](LICENSE).