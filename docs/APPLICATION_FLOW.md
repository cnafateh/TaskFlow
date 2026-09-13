# TaskFlow Application Flow

## Registration and Login

```text
Register
  ↓
ASP.NET Core Identity
  ↓
ApplicationUser created
  ↓
Authentication cookie
  ↓
User workspace
```

## Project Creation

```text
Authenticated User
      ↓
Projects/Create
      ↓
ProjectsController
      ↓
ProjectService
      ↓
Current UserId assigned server-side
      ↓
Project saved
```

Ownership is never trusted from client input.

## Category Creation

```text
Authenticated User
      ↓
Categories/Create
      ↓
CategoriesController
      ↓
CategoryService
      ↓
Current UserId assigned server-side
      ↓
Category saved
```

## Task Creation

A Task must reference a Project and Category owned by the current user.

```text
Create Task Form
      ↓
ProjectId + CategoryId
      ↓
TaskService
      ↓
Validate Project ownership
      ↓
Validate Category ownership
      ↓
Create Task
```

This prevents a user from assigning a Task to another user's resources by modifying the request.

## Task Management

Users can:

- create tasks
- edit tasks
- delete tasks
- view task details
- search
- filter
- sort
- paginate

Task status:

```text
Todo
InProgress
Done
```

Task priority:

```text
Low
Medium
High
```

## Administration

```text
/Admin
  ├── Users
  ├── Tasks
  ├── Projects
  └── Categories
```

### Users

Administrators can:

- search by email
- filter by role
- sort users
- paginate
- change another user's role

Safeguards include:

- an admin cannot change their own role
- the last administrator cannot be downgraded
- allowed role values are validated server-side

### Tasks

Administrators can search and filter all tasks by:

- owner
- project
- category
- status
- priority
- search text
- sort order

Bulk actions:

```text
Mark as Todo
Mark as In Progress
Mark as Done
Delete
```

### Projects

Administrators can search, filter, paginate, delete, and bulk-delete projects.

Deleting a project also deletes its tasks.

### Categories

Administrators can search, filter, paginate, and delete categories that are not currently in use.

## Error Handling

- unauthorized access → Access Denied
- unknown route → custom 404
- unexpected production exception → generic application error page

## Startup

```text
Application starts
      ↓
Migrations applied
      ↓
Roles ensured
      ↓
Bootstrap admin configuration checked
      ↓
Admin created if missing
```

An existing admin password is never reset automatically during startup.
