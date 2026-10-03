# TaskFlow Security

## 1. Authentication

TaskFlow uses ASP.NET Core Identity.

Authentication answers:

```text
Who is the current user?
```

The MVC application currently uses cookie authentication.

---

## 2. Authorization

Authorization answers:

```text
What is the current user allowed to do?
```

Roles:

```text
Admin
User
```

Administrative functionality requires the `Admin` role.

Authentication by itself does not authorize access to another user's data.

---

## 3. Ownership Rules

Project ownership:

```text
Project.UserId == CurrentUserId
```

Category ownership:

```text
Category.UserId == CurrentUserId
```

Task ownership:

```text
Task.Project.UserId == CurrentUserId
```

Task ownership is derived through the Project instead of storing a second Task-level ownership value.

---

## 4. Server-Side Ownership Enforcement

Ownership must never be trusted from client input.

For Task creation and editing:

```text
submitted ProjectId
      ↓
server-side ownership validation

submitted CategoryId
      ↓
server-side ownership validation
```

A user must not be able to access another user's data by modifying route, query, or form identifiers.

---

## 5. Application Service Enforcement

Ownership-sensitive use cases are implemented in Application services.

Example:

```text
TaskService
      ↓
IProjectRepository
      ↓
Project belongs to current user?

TaskService
      ↓
ICategoryRepository
      ↓
Category belongs to current user?
```

This behavior can be shared by MVC and the future REST API.

---

## 6. Identity Boundary

`ApplicationUser` belongs to Infrastructure because it derives from:

```text
IdentityUser
```

That makes it dependent on ASP.NET Core Identity.

Domain entities therefore do not depend directly on `ApplicationUser`.

---

## 7. Role Management

Role-management safeguards include:

- explicit validation of allowed role values
- prevention of administrator self-demotion
- protection against removing the final administrator
- server-side role transitions

---

## 8. Bootstrap Administrator

Startup can create or ensure a configured administrator account.

The bootstrap process:

- creates the account only when needed
- ensures the Admin role
- does not reset an existing password
- receives credentials from application configuration
- does not store credentials in source control

`Program.cs` reads configuration and passes only the required values to the Infrastructure seeder.

---

## 9. Development Secrets

Development uses:

```text
.NET User Secrets
```

Examples:

```text
SeedAdmin:Email
SeedAdmin:Password
```

These values are stored outside the repository.

---

## 10. Production Secrets

Production should use deployment/environment secrets.

Examples:

```text
SeedAdmin__Email
SeedAdmin__Password
TASKFLOW_DB_PASSWORD
```

Secrets must not be committed to Git.

---

## 11. Anti-Forgery

MVC state-changing form operations use ASP.NET Core anti-forgery validation.

Destructive state changes should not be performed through GET requests.

---

## 12. Database Security

Production uses a dedicated PostgreSQL application role.

The TaskFlow application should not use the PostgreSQL administrative account.

Database traffic occurs through an internal Docker network.

---

## 13. Data Protection

ASP.NET Core Data Protection keys are persisted outside the disposable application container.

Production path:

```text
/app/keys
```

This keeps authentication cookies valid across normal container recreation.

---

## 14. Error Handling

Production uses generic user-facing error pages.

Detailed technical information belongs in logs rather than responses.

Trace identifiers allow errors to be correlated with logs.

Sensitive data such as the following should not intentionally be logged:

- passwords
- cookies
- bearer tokens
- secrets
- database credentials

---

## 15. Administrative Operations

Administrative operations intentionally support cross-user access but require the Admin role.

Bulk operations treat submitted IDs only as selectors.

The server loads the matching entities before modification or deletion.

---

## 16. Dependency Security

The project uses NuGet packages for framework and infrastructure functionality.

Dependency vulnerability checks should remain part of project maintenance.

A NuGet vulnerability metadata warning caused by temporary inability to reach `api.nuget.org` does not by itself indicate a package vulnerability, but dependency metadata should be rechecked when network access is available.

---

## 17. REST API Security Roadmap

The future REST API phase requires decisions around:

- bearer authentication
- JWT/token strategy
- API authorization
- ownership enforcement
- API DTO validation
- HTTP error responses
- CORS
- rate limiting
- security headers
- OpenAPI exposure
- authentication integration tests

API security will reuse shared Application ownership rules instead of reimplementing them in controllers.

---

## 18. Remaining Hardening Work

Before treating TaskFlow as fully production-hardened:

- perform a complete authorization audit
- expand automated authorization tests
- review Identity password policies
- review account lockout behavior
- review rate limiting
- review security headers
- automate dependency scanning
- test backup restoration
- review reverse-proxy TLS configuration
- test API security once the API is introduced