# TaskFlow Architecture

## 1. Overview

TaskFlow is structured around explicit architectural boundaries.

The goal is not simply to divide code into projects.

The goal is to make the direction of dependencies intentional so reusable application behavior remains independent from HTTP, MVC, EF Core, database providers, and other technical details.

The main projects are:

```text
TaskFlow.Domain
TaskFlow.Application
TaskFlow.Infrastructure
TaskFlow.Web
TaskFlow.Tests
```

---

## 2. Dependency Rule

The intended source-code dependency graph is:

```text
                    Domain
                      ▲
                      │
                 Application
                  ▲        ▲
                  │        │
                Web   Infrastructure
```

Another way to express it:

```text
Application
    → Domain

Infrastructure
    → Application
    → Domain

Web
    → Application
    → Domain
    → Infrastructure
```

The important restrictions are:

```text
Domain
    ✗ does not depend on Application
    ✗ does not depend on Infrastructure
    ✗ does not depend on Web

Application
    ✗ does not depend on Infrastructure
    ✗ does not depend on Web
    ✗ does not depend on EF Core
```

Infrastructure may depend on Application because Infrastructure implements contracts owned by Application.

Web may reference Infrastructure because the composition root must register concrete implementations.

---

## 3. Domain Layer

Project:

```text
TaskFlow.Domain
```

Domain contains the core business concepts.

Current entities:

```text
TaskItem
Project
Category
```

Current enums:

```text
TaskPriority
TaskStatus
```

Domain should answer questions such as:

```text
What concepts exist in this problem?
What state belongs to those concepts?
Which rules intrinsically belong to those concepts?
```

Domain does not know about:

- HTTP
- MVC
- Razor
- Entity Framework Core
- PostgreSQL
- SQLite
- ASP.NET Core Identity
- Docker
- API controllers

This is the most independent layer.

---

## 4. Application Layer

Project:

```text
TaskFlow.Application
```

Application represents application use cases and reusable application behavior.

Current service contracts:

```text
ITaskService
IProjectService
ICategoryService
```

Current service implementations:

```text
TaskService
ProjectService
CategoryService
```

Current persistence contracts:

```text
ITaskRepository
IProjectRepository
ICategoryRepository
```

Other reusable application types include:

```text
TaskFilter
TaskSummary
PagedResult<T>
```

A useful distinction is:

```text
Domain
= core business concepts and intrinsic rules

Application
= use cases and orchestration
```

For example:

```text
Create Task
    ↓
validate Project ownership
    ↓
validate Category ownership
    ↓
store Task
```

is an application use case.

The Application layer coordinates that behavior.

It does not need to know how EF Core queries are written.

---

## 5. Infrastructure Layer

Project:

```text
TaskFlow.Infrastructure
```

Infrastructure contains technical details.

Current structure:

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

Infrastructure answers:

```text
How is persistence actually implemented?
How is authentication persistence implemented?
Which EF Core provider is used?
How is the database accessed?
```

---

## 6. Repository Contracts

Repository interfaces belong to Application.

Examples:

```text
ITaskRepository
IProjectRepository
ICategoryRepository
```

Application owns these interfaces because Application owns the requirements.

For example, `TaskService` may need to ask:

```text
Does this Project belong to this user?
Does this Category belong to this user?
Load this Task for this user.
Store this Task.
```

Application describes those capabilities through interfaces.

It does not describe EF Core implementation details.

---

## 7. Repository Implementations

Infrastructure implements the repository contracts.

Example:

```text
Application
└── ITaskRepository
        ▲
        │ implements
Infrastructure
└── TaskRepository
```

`TaskRepository` can use:

```text
AppDbContext
DbSet<T>
Include
Where
AnyAsync
CountAsync
Skip
Take
SaveChangesAsync
```

These are persistence details and therefore belong to Infrastructure.

---

## 8. Application Services

Application services decide what should happen during a use case.

Example dependency graph:

```text
TaskService
├── ITaskRepository
├── IProjectRepository
└── ICategoryRepository
```

During Task creation:

```text
TaskService
   ↓
IProjectRepository
   ↓
validate Project ownership

TaskService
   ↓
ICategoryRepository
   ↓
validate Category ownership

TaskService
   ↓
ITaskRepository
   ↓
store Task
```

`TaskService` does not access `AppDbContext`.

This is a major architectural boundary.

---

## 9. Runtime Request Flow

For normal MVC Task operations:

```text
Browser
   ↓
ASP.NET Core middleware
   ↓
Routing
   ↓
Authentication
   ↓
Authorization
   ↓
TasksController
   ↓
ITaskService
   ↓
TaskService
   ↓
Repository Interfaces
   ↓
Repository Implementations
   ↓
AppDbContext
   ↓
EF Core
   ↓
Database
```

The Controller does not directly query the database.

The Application Service does not directly query EF Core.

---

## 10. Compile-Time Dependencies vs Runtime Calls

This distinction is central to the architecture.

### Compile-time

Application knows:

```text
ITaskRepository
```

Application does not know:

```text
TaskRepository
AppDbContext
EF Core
PostgreSQL
```

Infrastructure knows the Application interface because it implements it.

Therefore the source-code dependency is:

```text
Infrastructure
      ↓
Application
```

### Runtime

During execution:

```text
TaskService
   ↓
ITaskRepository
   ↓
TaskRepository
   ↓
AppDbContext
```

The DI container connects the interface to the implementation.

Compile-time dependency direction and runtime call flow are therefore not identical.

---

## 11. Dependency Inversion

Without dependency inversion:

```text
TaskService
   ↓
TaskRepository
   ↓
AppDbContext
```

Application would depend directly on technical infrastructure.

With dependency inversion:

```text
Application
┌───────────────────┐
│ TaskService       │
│       ↓           │
│ ITaskRepository   │
└────────▲──────────┘
         │
         │ implements
         │
Infrastructure
┌────────┴──────────┐
│ TaskRepository   │
│       ↓          │
│ AppDbContext     │
└──────────────────┘
```

The high-level Application code owns the abstraction.

The lower-level Infrastructure code implements it.

---

## 12. Dependency Injection

ASP.NET Core's dependency injection container constructs the object graph.

Example registrations conceptually include:

```csharp
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
```

Human translation:

```text
When something asks for ITaskService,
provide TaskService.

When something asks for ITaskRepository,
provide TaskRepository.
```

If `TaskRepository` needs an `AppDbContext`, DI resolves that dependency too.

---

## 13. Scoped Lifetime

Database-oriented services and repositories use request-scoped dependencies.

Conceptually:

```text
HTTP Request starts
      ↓
DI scope created
      ↓
AppDbContext created
      ↓
services/repositories resolved
      ↓
request completes
      ↓
scope disposed
```

This works naturally with EF Core's `DbContext`.

---

## 14. Tracked and Read-Only Queries

Read-only operations can use:

```text
AsNoTracking
```

because EF Core does not need to track changes.

Update and Delete operations require tracked entities.

This is why repository contracts can distinguish between:

```text
GetByIdForUserAsync
```

and:

```text
GetTrackedByIdForUserAsync
```

The first is appropriate for read-only operations.

The second is appropriate when a loaded entity will be modified or deleted and later persisted using `SaveChangesAsync`.

---

## 15. AppDbContext

`AppDbContext` belongs to Infrastructure.

Application does not reference it.

Development uses:

```text
SQLite
```

Production uses:

```text
PostgreSQL
```

The Application layer remains unchanged when the provider changes.

---

## 16. Identity

`ApplicationUser` belongs to Infrastructure because it inherits from:

```text
IdentityUser
```

That ties it to ASP.NET Core Identity.

Domain entities therefore do not depend directly on `ApplicationUser`.

Project and Category store the scalar ownership identifier they need.

---

## 17. Configuration Boundary

Infrastructure's `IdentitySeeder` does not receive the entire `IConfiguration`.

Instead:

```text
Program.cs
   ↓
reads configuration
   ↓
email/password values
   ↓
IdentitySeeder
```

This keeps configuration-source knowledge at the application edge.

---

## 18. Web Layer

`TaskFlow.Web` contains MVC-specific responsibilities.

Examples:

- HTTP requests
- Controllers
- Razor Views
- MVC ViewModels
- TempData
- status-code pages
- presentation filters
- request pipeline
- composition root

Controllers should translate HTTP input into Application calls.

They should not implement persistence logic.

---

## 19. MVC ViewModels

ViewModels are presentation contracts.

They may contain UI-specific concerns such as:

- form fields
- SelectList items
- table state
- MVC validation requirements
- display formatting

A future REST API will use API-specific DTOs instead of reusing MVC ViewModels.

---

## 20. AdminService

`AdminService` currently remains associated with the Web/Admin area.

This is intentional.

The current implementation contains concerns strongly coupled to:

- ASP.NET Core Identity
- administrative MVC behavior
- admin-specific ViewModels
- cross-user management

The architecture does not move code between layers merely to create symmetry.

If reusable administrative use cases are required by the future API, those shared use cases can be extracted deliberately.

---

## 21. Generic Repository

TaskFlow does not introduce a generic:

```text
IRepository<T>
```

only to wrap EF Core CRUD operations.

EF Core already provides repository/unit-of-work-like behavior through:

```text
DbSet<T>
DbContext
```

TaskFlow instead uses focused repository contracts that express specific Application requirements.

---

## 22. MVC and REST API

The architecture is now ready for another presentation entry point.

Target structure:

```text
             ┌───────────────┐
             │ TaskFlow.Web  │
             │ MVC           │
             └───────┬───────┘
                     │
                     ▼
              ┌─────────────┐
              │ Application │
              └──────▲──────┘
                     │
             ┌───────┴───────┐
             │ TaskFlow.Api  │
             │ REST API      │
             └───────────────┘
```

Both presentation layers will use the same Application services.

The API will not call the MVC application.

The MVC application will not call the API internally.

---

## 23. Pre-API Architecture Status

```text
Domain separation                      ✅
Application separation                 ✅
Infrastructure separation              ✅
AppDbContext extraction                ✅
Identity extraction                    ✅
Migration extraction                   ✅
Application service extraction         ✅
Repository contracts                   ✅
Repository implementations             ✅
Dependency inversion                   ✅
Application independent of EF Core     ✅
Application independent of Web         ✅
Application independent of Infrastructure ✅
Build                                  ✅
Automated tests                        ✅
REST API                               ⏳
```

The next architecture phase is the introduction of the REST API presentation layer.