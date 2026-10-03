# TaskFlow Dependency Flow

This document explains exactly how TaskFlow projects, services, repositories, dependency injection, and runtime calls relate to each other.

---

## 1. Two Different Flows

TaskFlow has two different concepts that should not be confused:

```text
Source-code dependency direction
```

and:

```text
Runtime execution flow
```

They are related, but they are not the same thing.

---

# 2. Source-Code Dependency Direction

The architectural dependency rule is:

```text
                    Domain
                      ▲
                      │
                 Application
                  ▲        ▲
                  │        │
                Web   Infrastructure
```

Meaning:

```text
Application references Domain.

Infrastructure references Application and Domain.

Web references the layers it needs to compose and execute the application.
```

The important restriction is:

```text
Application
    ✗ must not reference Infrastructure
    ✗ must not reference Web
```

---

# 3. Domain

Domain is the innermost layer.

```text
TaskFlow.Domain
```

It contains:

```text
Entities
Enums
core business concepts
```

Examples:

```text
TaskItem
Project
Category
TaskPriority
TaskStatus
```

Domain does not know how its data is stored or exposed.

---

# 4. Application

```text
TaskFlow.Application
```

Application contains use cases.

Examples:

```text
TaskService
ProjectService
CategoryService
```

It also defines the abstractions required to perform those use cases:

```text
ITaskRepository
IProjectRepository
ICategoryRepository
```

Application says:

```text
I need a way to load this Task.
I need to know whether this Project belongs to this user.
I need to store this Category.
```

It does not say:

```text
Use EF Core.
Use PostgreSQL.
Call DbContext.
```

---

# 5. Infrastructure

```text
TaskFlow.Infrastructure
```

Infrastructure fulfills technical contracts.

Example:

```text
Application:
ITaskRepository

Infrastructure:
TaskRepository
```

`TaskRepository` uses EF Core.

---

# 6. Port and Adapter

The repository interface can be viewed as a Port.

```text
ITaskRepository
= Port
```

The EF Core implementation is an Adapter.

```text
TaskRepository
= Adapter
```

So:

```text
Application Port
       ▲
       │ implemented by
       │
Infrastructure Adapter
```

---

# 7. Task Runtime Flow

A Task request flows like this:

```text
Browser
   ↓
TasksController
   ↓
ITaskService
   ↓
TaskService
```

TaskService may then coordinate several repositories:

```text
TaskService
├── ITaskRepository
├── IProjectRepository
└── ICategoryRepository
```

Those contracts resolve to Infrastructure implementations:

```text
ITaskRepository
      ↓
TaskRepository

IProjectRepository
      ↓
ProjectRepository

ICategoryRepository
      ↓
CategoryRepository
```

The repositories access:

```text
AppDbContext
      ↓
EF Core
      ↓
Database
```

Combined flow:

```text
TasksController
      ↓
TaskService
      ↓
┌───────────────────────────────┐
│ ITaskRepository               │
│ IProjectRepository            │
│ ICategoryRepository           │
└───────────────┬───────────────┘
                ↓
┌───────────────────────────────┐
│ TaskRepository                │
│ ProjectRepository             │
│ CategoryRepository            │
└───────────────┬───────────────┘
                ↓
          AppDbContext
                ↓
             EF Core
                ↓
            Database
```

---

# 8. Why TaskService Uses Multiple Repositories

TaskService owns the Task use case.

For example:

```text
Create Task
```

requires more than simply inserting a Task row.

The flow is:

```text
Receive Task
   ↓
check Project belongs to user
   ↓
check Category belongs to user
   ↓
add Task
   ↓
save changes
```

Those responsibilities map naturally to:

```text
IProjectRepository
ICategoryRepository
ITaskRepository
```

TaskService coordinates them.

This coordination is Application logic.

---

# 9. Service vs Repository

A useful mental model:

```text
Service
= decides what should happen

Repository
= performs persistence operations
```

Example:

```text
TaskService:
"Only create the Task when Project and Category are valid for this user."

Repository:
"Check the database for the Project."
"Check the database for the Category."
"Add the Task."
```

---

# 10. Compile-Time Flow

`TaskService` contains something conceptually like:

```csharp
private readonly ITaskRepository _taskRepository;
```

It does not contain:

```csharp
private readonly TaskRepository _taskRepository;
```

and it does not contain:

```csharp
private readonly AppDbContext _context;
```

Therefore Application only depends on its abstraction.

Infrastructure contains:

```csharp
public class TaskRepository : ITaskRepository
```

So compile-time dependency points inward:

```text
Infrastructure
      ↓
Application
      ↓
Domain
```

---

# 11. Runtime Flow

At runtime, calls move through the concrete object graph:

```text
TaskService
   ↓
ITaskRepository
   ↓
TaskRepository
   ↓
AppDbContext
```

The Application project still does not reference Infrastructure.

Dependency Injection performs the connection.

---

# 12. Dependency Injection

The composition root registers abstractions and implementations.

Conceptually:

```csharp
builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
```

ASP.NET Core interprets:

```text
Need ITaskService?
→ create TaskService.

TaskService needs ITaskRepository?
→ create TaskRepository.

TaskRepository needs AppDbContext?
→ resolve AppDbContext.
```

This process is recursive.

---

# 13. Object Graph

For a Task request the DI container may build:

```text
TasksController
      ↓
TaskService
      ├── TaskRepository
      │       └── AppDbContext
      │
      ├── ProjectRepository
      │       └── AppDbContext
      │
      └── CategoryRepository
              └── AppDbContext
```

Because the dependencies are scoped, the repositories participating in the same HTTP request operate in the same request scope.

---

# 14. Scoped Lifetime

Conceptually:

```text
HTTP request begins
      ↓
dependency injection scope created
      ↓
AppDbContext resolved
      ↓
repositories resolved
      ↓
services resolved
      ↓
controller executes
      ↓
request completes
      ↓
scope disposed
```

This matches EF Core's recommended request-based use of `DbContext`.

---

# 15. Read vs Update Queries

A read-only query can use:

```text
AsNoTracking
```

Example:

```text
GetByIdForUserAsync
```

An update or delete operation requires a tracked entity:

```text
GetTrackedByIdForUserAsync
```

Flow:

```text
Load tracked entity
      ↓
change properties
      ↓
SaveChangesAsync
```

---

# 16. SaveChanges

Application never calls:

```text
AppDbContext.SaveChangesAsync()
```

directly.

Instead it calls through its persistence abstraction.

Example:

```text
ITaskRepository.SaveChangesAsync()
```

Infrastructure implements that operation with `AppDbContext`.

---

# 17. Project Flow

```text
ProjectsController
      ↓
IProjectService
      ↓
ProjectService
      ↓
IProjectRepository
      ↓
ProjectRepository
      ↓
AppDbContext
```

---

# 18. Category Flow

```text
CategoriesController
      ↓
ICategoryService
      ↓
CategoryService
      ↓
ICategoryRepository
      ↓
CategoryRepository
      ↓
AppDbContext
```

---

# 19. MVC-Specific Flow

MVC is currently one entry point.

```text
HTTP Form / Query / Route
      ↓
MVC Controller
      ↓
Application Service
      ↓
Application result
      ↓
Controller
      ↓
View / Redirect / TempData
```

MVC ViewModels remain in Web because they are presentation-specific.

---

# 20. Future REST API Flow

The future API will be another entry point:

```text
HTTP JSON Request
      ↓
API Controller
      ↓
Application Service
      ↓
Repository Contracts
      ↓
Infrastructure
      ↓
Database
```

MVC and API therefore share:

```text
Domain
Application
Infrastructure
```

but have different presentation concerns.

---

# 21. MVC vs API

MVC:

```text
Input:
HTML form / route / query

Output:
View / redirect / HTML
```

API:

```text
Input:
JSON / route / query

Output:
JSON / HTTP status codes
```

Application:

```text
does not care which one called it
```

---

# 22. Why the API Does Not Call MVC

The intended architecture is not:

```text
API
 ↓
MVC
 ↓
Application
```

and not:

```text
MVC
 ↓
API
```

Instead:

```text
MVC ──────┐
          ▼
     Application
          ▲
API ──────┘
```

MVC and API are peers.

---

# 23. Why Application Does Not Know EF Core

If Application directly referenced EF Core:

```text
Application
   ↓
Entity Framework Core
```

then persistence technology would become part of Application.

TaskFlow instead uses:

```text
Application
   ↓
Repository Interfaces

Infrastructure
   ↓
EF Core
```

This keeps the Application use cases independent from persistence technology.

---

# 24. Key Vocabulary

## Domain

Core business concepts and intrinsic rules.

## Application

Use cases and orchestration.

## Infrastructure

Technical implementations.

## Presentation

How users or clients interact with the system.

## Interface

A contract describing required behavior.

## Implementation

Concrete code satisfying a contract.

## Repository

A persistence boundary.

## Dependency Injection

Dependencies are supplied to objects instead of constructed internally.

## Dependency Inversion

High-level Application code depends on abstractions while lower-level technical code implements those abstractions.

## Composition Root

The place where concrete dependencies are connected.

Current MVC composition root:

```text
TaskFlow.Web/Program.cs
```

---

# 25. Final Pre-API Dependency Model

```text
                         Domain
                           ▲
                           │
                      Application
                  ┌────────┴────────┐
                  ▲                 ▲
                  │                 │
               Web            Infrastructure
```

Runtime:

```text
Web Controller
      ↓
Application Service
      ↓
Application Repository Interface
      ↓
Infrastructure Repository
      ↓
AppDbContext
      ↓
Database
```

This is the architecture used as the foundation for the REST API phase.