# TaskFlow Application Flow

## 1. MVC Request Entry

A browser request enters TaskFlow through ASP.NET Core.

```text
Browser
   ↓
Reverse Proxy (production)
   ↓
ASP.NET Core
   ↓
Middleware
   ↓
Routing
   ↓
Authentication
   ↓
Authorization
   ↓
Controller
```

Controllers translate HTTP-specific input into Application operations.

---

## 2. Registration and Login

```text
Register / Login
      ↓
ASP.NET Core Identity
      ↓
ApplicationUser
      ↓
Authentication Cookie
      ↓
Authenticated Request
```

`ApplicationUser` belongs to Infrastructure because it derives from ASP.NET Core Identity's `IdentityUser`.

---

## 3. Project Creation

```text
Authenticated User
      ↓
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
      ↓
Database
```

The current user's identifier is assigned server-side.

Ownership is not trusted from client input.

---

## 4. Category Creation

```text
Authenticated User
      ↓
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
      ↓
Database
```

Category ownership is assigned server-side.

---

## 5. Task Creation

A Task must reference a Project and Category available to the authenticated user.

```text
Create Task Form
      ↓
TasksController
      ↓
ITaskService
      ↓
TaskService
```

TaskService coordinates the use case:

```text
TaskService
      ↓
IProjectRepository
      ↓
Project belongs to current user?
      ↓
ICategoryRepository
      ↓
Category belongs to current user?
      ↓
ITaskRepository
      ↓
Add Task
      ↓
Save Changes
```

This prevents a user from assigning a Task to another user's resources by changing submitted identifiers.

---

## 6. Task Ownership

Task ownership is derived through Project ownership.

```text
Task
  ↓
Project
  ↓
UserId
```

Rule:

```text
Task.Project.UserId == CurrentUserId
```

Task does not store a duplicate user ownership field.

---

## 7. Task Details

```text
TasksController
      ↓
ITaskService
      ↓
TaskService
      ↓
ITaskRepository
      ↓
GetByIdForUserAsync
      ↓
TaskRepository
      ↓
database query
```

Read-only queries may use `AsNoTracking`.

---

## 8. Task Update

```text
Edit Request
      ↓
TasksController
      ↓
TaskService
      ↓
load tracked Task for current user
      ↓
validate target Project
      ↓
validate target Category
      ↓
update Task fields
      ↓
SaveChanges
```

A Task owned by another user is not returned for modification.

---

## 9. Task Delete

```text
Delete Request
      ↓
TasksController
      ↓
TaskService
      ↓
load tracked Task for current user
      ↓
remove Task
      ↓
SaveChanges
```

---

## 10. Task Filtering

Users can:

- search
- filter
- sort
- paginate

Flow:

```text
HTTP Query Parameters
      ↓
TaskFilter
      ↓
TaskService
      ↓
ITaskRepository
      ↓
TaskRepository
      ↓
IQueryable
      ↓
Search
      ↓
Filters
      ↓
Sort
      ↓
Count
      ↓
Skip / Take
      ↓
PagedResult<TaskItem>
```

Filtering and pagination occur in the database rather than loading all Tasks into application memory.

---

## 11. Task Summary

```text
Controller / View Component
      ↓
ITaskService
      ↓
TaskService
      ↓
ITaskRepository
      ↓
TaskRepository
      ↓
database counts
      ↓
TaskSummary
```

Summary values are restricted to the current user's Tasks.

---

## 12. Task Status

```text
Todo
InProgress
Done
```

---

## 13. Task Priority

```text
Low
Medium
High
```

---

## 14. Project Update

```text
ProjectsController
      ↓
ProjectService
      ↓
IProjectRepository
      ↓
tracked Project owned by current user
      ↓
modify fields
      ↓
SaveChanges
```

---

## 15. Category Update

```text
CategoriesController
      ↓
CategoryService
      ↓
ICategoryRepository
      ↓
tracked Category owned by current user
      ↓
modify fields
      ↓
SaveChanges
```

---

## 16. Category Delete Rule

Before Category deletion, TaskFlow can determine whether the Category is currently used by Tasks.

```text
CategoryService
      ↓
ICategoryRepository
      ↓
HasTasksForUserAsync
```

A Category referenced by Tasks is protected from normal deletion.

---

## 17. Administration

Administrative functionality includes:

```text
Admin
├── Users
├── Tasks
├── Projects
└── Categories
```

Admin operations intentionally support cross-user management and differ from normal user ownership-restricted flows.

---

## 18. Admin User Management

Administrators can:

- search users
- filter by role
- sort
- paginate
- change another user's role

Safeguards include:

```text
admin cannot change own role
last administrator cannot be downgraded
allowed role values are validated
```

---

## 19. Admin Task Operations

Administrators can:

- search all Tasks
- filter by owner
- filter by Project
- filter by Category
- filter by status
- filter by priority
- sort
- paginate
- perform bulk operations

Bulk actions include:

```text
Mark Todo
Mark In Progress
Mark Done
Delete
```

Posted identifiers are treated only as selectors.

The server loads the actual matching entities before modifying them.

---

## 20. Error Handling

```text
Unauthorized operation
      ↓
Access Denied

Unknown route/resource
      ↓
Custom 404

Unexpected production exception
      ↓
Global exception handler
      ↓
Generic error page
      ↓
Trace identifier
```

---

## 21. Startup Flow

Production startup:

```text
Application starts
      ↓
configuration loaded
      ↓
dependency registrations created
      ↓
request pipeline configured
      ↓
AppDbContext resolved
      ↓
pending EF migrations applied
      ↓
Identity roles ensured
      ↓
bootstrap admin values read
      ↓
IdentitySeeder invoked
      ↓
application begins serving requests
```

The bootstrap administrator's existing password is not reset automatically.

---

## 22. Development Database Startup

Development uses SQLite.

The local database can be created from the current EF Core model.

Production instead uses PostgreSQL migrations.

---

## 23. Future API Flow

The next phase introduces a second presentation entry point:

```text
REST Client
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

The API will reuse the same Application services used by MVC.

No duplicate Task, Project, or Category business/application workflows are required.