# TaskFlow Security

## Authentication

TaskFlow uses ASP.NET Core Identity.

Authentication cookies use ASP.NET Core Data Protection. Production Data Protection keys are persisted outside the container so authentication remains stable across container recreation.

## Authorization

Administrative functionality is restricted to the `Admin` role.

Normal user operations also enforce ownership. Authentication alone does not authorize a user to access another user's data.

## Ownership Rules

### Project

```text
Project.UserId == CurrentUserId
```

### Category

```text
Category.UserId == CurrentUserId
```

### Task

```text
Task.Project.UserId == CurrentUserId
```

## Overposting Protection

Ownership values are assigned or validated server-side.

For Task creation and editing:

- submitted `ProjectId` must belong to the current user
- submitted `CategoryId` must belong to the current user
- ownership must not be taken from arbitrary form values

## Administrative Operations

Admin operations intentionally bypass normal per-user restrictions but require the Admin role.

Bulk actions accept IDs as selectors only. The server queries the actual entities from the database before changing or deleting them.

## Role Management

Role changes include safeguards:

- allowed role values are explicitly validated
- an administrator cannot change their own role
- the final administrator cannot be downgraded
- the target role is added before old roles are removed
- failed transitions attempt rollback

## Anti-Forgery

Destructive POST operations use ASP.NET Core anti-forgery validation.

GET requests are not used for destructive state changes.

## Secrets

Development uses .NET User Secrets.

Production should use:

- container environment variables
- deployment-platform secrets
- a dedicated secret store

Expected bootstrap settings:

```text
SeedAdmin:Email
SeedAdmin:Password
```

Environment variable equivalents:

```text
SeedAdmin__Email
SeedAdmin__Password
```

## Bootstrap Administrator

The seeder:

- creates the configured admin only when missing
- ensures the Admin role
- does not reset an existing password on startup

The bootstrap password should therefore be treated as initial provisioning data.

## Docker Runtime

The application runs under the non-root .NET container user.

Persistent writable locations are:

```text
/app/data
/app/keys
```

## Error Handling

Production mode uses a generic error page. Stack traces and development exception details should not be exposed publicly.

## Remaining Security Work

Before treating TaskFlow as production-hardened, complete:

- systematic ownership audit of controller actions
- rate limiting review
- password and lockout policy review
- security-header review
- automated authorization tests
- dependency vulnerability scanning
- backup and recovery testing
- reverse-proxy HTTPS hardening
