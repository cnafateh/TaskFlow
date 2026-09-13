# TaskFlow Architecture

## Overview

TaskFlow is an ASP.NET Core MVC application organized around controllers, services, Entity Framework Core, ASP.NET Core Identity, and Razor views.

The goal is to keep HTTP concerns, business rules, persistence, and presentation responsibilities sufficiently separated so the application remains understandable and extensible.

## Request Flow

```text
Browser
  ↓
ASP.NET Core Routing
  ↓
Authentication / Authorization
  ↓
Controller
  ↓
Service
  ↓
AppDbContext / Entity Framework Core
  ↓
SQLite
```

## Controllers

Controllers are responsible for:

- receiving HTTP requests
- reading route, form, and query parameters
- resolving the authenticated user
- calling services
- returning views or redirects
- setting TempData messages

Persistence logic belongs in services rather than controllers.

## Services

The main service areas are:

- `TaskService`
- `ProjectService`
- `CategoryService`
- `AdminService`

User-facing services enforce ownership rules. `AdminService` contains privileged cross-user operations for administrators.

## Data Model

The main relationships are:

```text
ApplicationUser
  ├── Projects
  │     └── Tasks
  │
  └── Categories
        └── Tasks
```

A Task belongs to one Project and one Category.

Task ownership is derived from the Task's Project.

## ViewModels

ViewModels isolate UI contracts from persistence entities where appropriate.

They are used for:

- task create/edit/delete flows
- admin index/filter pages
- paginated admin row models
- task summaries
- filter state

## Authentication and Authorization

TaskFlow uses ASP.NET Core Identity with a custom `ApplicationUser`.

Roles:

```text
Admin
User
```

Administrative controllers are protected through role-based authorization. Normal application resources additionally enforce ownership.

## Ownership

### Projects

```text
Project.UserId == CurrentUserId
```

### Categories

```text
Category.UserId == CurrentUserId
```

### Tasks

```text
Task.Project.UserId == CurrentUserId
```

Task ownership is intentionally derived through Project ownership rather than storing an additional Task `UserId`.

## Delete Behavior

### Project → Task

Project deletion cascades to its Tasks.

### Category → Task

Category deletion is restricted while Tasks reference that Category.

## Pagination and Filtering

List pages use filter objects and `PagedResult<T>`.

```text
Query Parameters
      ↓
Filter object
      ↓
IQueryable
      ↓
Search / Filter / Sort
      ↓
Count
      ↓
Skip / Take
      ↓
PagedResult<T>
```

This keeps filtering and pagination in the database.

## Administration

The administrative area supports:

- dashboard statistics
- user role management
- global task management
- task bulk actions
- global project management
- global category management

Bulk operations use posted IDs only as selectors. The server queries the actual matching entities before changing or deleting them.

## Startup Flow

```text
Application starts
      ↓
Services configured
      ↓
Request pipeline configured
      ↓
EF Core migrations applied
      ↓
Identity roles ensured
      ↓
Bootstrap admin ensured if configured
      ↓
Application serves requests
```

## Container Architecture

```text
ASP.NET Core container
   ├── /app/data
   │      └── taskflow.db
   │
   └── /app/keys
          └── Data Protection keys
```

Both locations use persistent Docker volumes. The application process runs as the non-root .NET container user.
