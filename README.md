# Payroll Anomaly Dashboard

![CI](https://github.com/clam920/PayrollAnomalyDashboard/actions/workflows/ci.yml/badge.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Postgres](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql&logoColor=white)

A backend API for payroll processing that automatically **flags anomalous timesheets** before they reach payroll — excessive hours, duplicate entries, unusually high pay, and more — with an approval workflow that requires a documented reason to override a flag, and JWT-based role authorization separating Manager and Employee actions.

Built to practice production-grade .NET/C# patterns: Clean Architecture, CQRS with MediatR, domain-driven design, and the cross-cutting concerns (validation, structured logging, centralized error handling, authentication) that separate a tutorial project from something closer to real engineering.

## Why this exists

Payroll errors are expensive and easy to miss — a typo'd hours field, a duplicate submission, an unapproved 18-hour shift. Most small systems either trust every submission or drown reviewers in noise. This project's core idea is a **pluggable rule-based anomaly detector** that runs automatically on every timesheet submission, paired with a review workflow that forces a human to *consciously* override a flagged item rather than silently approving it.

## Features

- **Automatic anomaly detection** on every timesheet submission — excessive hours, duplicate work dates, weekend work, and unusually high calculated pay
- **Payroll calculation** with regular/overtime (1.5×) pay split
- **Approve/reject workflow** — approving a flagged timesheet requires a non-empty override reason; rejecting a flagged one doesn't
- **JWT authentication with role-based authorization** — Manager vs Employee roles, enforced via ASP.NET Core policies
- **Dashboard summary endpoint** — anomaly counts by severity/type and a most-flagged-employees leaderboard
- **PostgreSQL** with EF Core migrations (schema changes are tracked, versioned files — not "delete the database and start over")
- **Input validation** via FluentValidation, running as a MediatR pipeline behavior before any handler executes
- **Centralized error handling** — one middleware maps domain/validation exceptions to consistent HTTP status codes instead of per-endpoint try/catch
- **Structured logging** via Serilog, including an audit trail for flagged-timesheet overrides
- **Three-tier automated test suite**: unit tests for domain logic, unit tests for application handlers (mocked), and real end-to-end integration tests against a containerized PostgreSQL instance via Testcontainers
- **Dockerized** with a multi-stage build and a `docker-compose.yml` for local Postgres + API; **CI** runs the full build + test suite (including the containerized integration tests) on every push via GitHub Actions

## Architecture

Clean Architecture, four layers, dependencies point inward:

```
Payroll.API              → ASP.NET Core minimal API, middleware, auth, DI composition root
Payroll.Application      → CQRS commands/queries (MediatR), validators, interfaces, auth contracts
Payroll.Domain           → Entities, value objects, domain services (no external dependencies)
Payroll.Infrastructure   → EF Core + PostgreSQL, JWT/password hashing implementations
```

The **anomaly detection** and **payroll calculation** rules live in `Payroll.Domain/Services` as pure, dependency-free domain services — they take plain objects in and return plain objects out, which is what makes them fully unit-testable without a database or mocking framework.

## Tech stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 8 (minimal APIs), Swagger/OpenAPI |
| Application | MediatR (CQRS), FluentValidation |
| Data | EF Core 8, PostgreSQL, code-first migrations |
| Auth | JWT Bearer tokens, role-based authorization policies |
| Logging | Serilog (structured console logging) |
| Testing | xUnit, Moq, Testcontainers, `WebApplicationFactory` |
| CI/CD | Docker (multi-stage build), Docker Compose, GitHub Actions |

## Authentication

Login issues a JWT carrying the user's role. Two demo accounts are seeded in memory on startup (see [Design notes](#design-notes) for why there's no real user database):

| Username | Password | Role |
|---|---|---|
| `manager` | `Manager123!` | Manager |
| `employee` | `Employee123!` | Employee |

```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"manager","password":"Manager123!"}'
```
Copy the returned `token` into Swagger's **Authorize** button (or an `Authorization: Bearer <token>` header) to call protected endpoints.

**Manager-only:** creating employees, approve/reject, dashboard summary.
**Any authenticated user:** everything else (viewing employees/timesheets/anomalies, submitting timesheets, viewing pay).

## API endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/login` | — | Log in, returns a JWT |
| `POST` | `/api/employees` | Manager | Create an employee |
| `GET` | `/api/employees` | Any | List all employees |
| `POST` | `/api/timesheets` | Any | Submit a timesheet — runs anomaly detection, returns the flagged count |
| `GET` | `/api/timesheets?employeeId=` | Any | List timesheets, optionally filtered by employee |
| `GET` | `/api/timesheets/{id}/pay` | Any | Regular/overtime hours and pay for one timesheet |
| `PUT` | `/api/timesheets/{id}/approve` | Manager | Approve a timesheet (requires `overrideReason` if flagged) |
| `PUT` | `/api/timesheets/{id}/reject` | Manager | Reject a timesheet |
| `GET` | `/api/anomalies?minSeverity=` | Any | List flagged anomalies, optionally filtered by minimum severity |
| `GET` | `/api/dashboard/summary?topEmployeeCount=` | Manager | Aggregate anomaly stats + most-flagged-employees leaderboard |

Full request/response schemas are available via Swagger UI when running in Development mode (see below).

## Getting started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) — required for PostgreSQL locally and for running the integration test suite

### Run locally
```bash
git clone https://github.com/clam920/PayrollAnomalyDashboard.git
cd PayrollAnomalyDashboard

# Start a local Postgres in Docker (see docker-compose.yml)
docker compose up -d postgres

dotnet run --project Payroll.API
```
Then open `https://localhost:{port}/swagger` (port is shown in the console output).

> The database schema is created/updated automatically on startup via EF Core's `Database.Migrate()`, applying the migration files in `Payroll.Infrastructure/Migrations/`. If you change an entity, generate a new migration: `dotnet ef migrations add <Name> --project Payroll.Infrastructure --startup-project Payroll.API`.

### Run with Docker (API + Postgres together)
```bash
docker compose up -d
```
Then open `http://localhost:8080/swagger`.

### Run tests
```bash
dotnet test
```
Runs all three test projects:
- `Payroll.Domain.Tests` — pure domain logic (no I/O)
- `Payroll.Application.Tests` — MediatR command/query handlers, with repositories mocked via Moq
- `Payroll.IntegrationTests` — real end-to-end HTTP tests (including the JWT login flow and role-based authorization) against a throwaway PostgreSQL container spun up automatically via [Testcontainers](https://testcontainers.com/). **Requires Docker to be running** — these tests fail immediately (not hang) if Docker isn't available. Expect the first run to take longer while Docker pulls the `postgres:16` image.

## Project structure
```
Payroll.API/
  Middleware/              Global exception-handling middleware
  Program.cs               Composition root, DI, auth config, endpoint mapping
Payroll.Application/
  Auth/                     Auth contracts (AppUser, IUserStore, IJwtTokenGenerator, JwtSettings)
  Behaviors/                MediatR validation pipeline
  Commands/                 Write operations + their validators
  Queries/                  Read operations
  Exceptions/               Application-level exception types
  Interfaces/               Repository contracts
Payroll.Domain/
  Entities/                 Employee, Timesheet, Anomaly
  Enums/                    AnomalyType, AnomalySeverity
  Services/                 Anomaly detection, payroll calculation (pure domain logic)
  ValueObjects/             PayCalculationResult
Payroll.Infrastructure/
  Auth/                     InMemoryUserStore, JwtTokenGenerator
  Repositories/             EF Core repository implementations
  Migrations/               EF Core migration history
  data/                     AppDbContext
Payroll.Domain.Tests/          Unit tests for domain entities & services
Payroll.Application.Tests/     Unit tests for command/query handlers (Moq)
Payroll.IntegrationTests/      End-to-end HTTP tests against a real, containerized Postgres
```

## Design notes

A few decisions worth calling out for anyone reviewing this code:

- **Anomaly rules are pure and testable.** `TimesheetAnomalyDetector` takes an `Employee`, a `Timesheet`, and the employee's recent history, and returns a list of `Anomaly` objects — no database, no mocking needed to test it.
- **The override rule lives in the Application layer, not the entity.** Requiring a reason to approve a flagged timesheet needs knowledge of both `Timesheet` and `Anomaly`, which a single entity shouldn't hold — that cross-aggregate rule lives in `ApproveTimesheetCommandHandler`.
- **Validation happens twice, on purpose.** FluentValidation catches malformed input at the API boundary with a clean 400; the domain entities' own constructor guard clauses are the last line of defense regardless of how they're constructed.
- **Auth is deliberately not a full user-management system.** `InMemoryUserStore` seeds two hardcoded demo accounts with passwords hashed at startup via `IPasswordHasher<T>` — there's no registration, password reset, or user database. The point of this piece of the project is real JWT issuance and role-based authorization working end-to-end, not a user-management system; swapping `InMemoryUserStore` for a real, database-backed implementation behind the same `IUserStore` interface wouldn't require changing anything else.
- **Integration tests use a real database, not a fake one.** `Payroll.IntegrationTests` boots the actual `Program.cs` DI container and hits real HTTP endpoints against a genuine (if throwaway) PostgreSQL instance via Testcontainers — this catches classes of bugs (DI registration gaps, EF Core query translation issues, exception-middleware wiring) that mocked unit tests structurally cannot.

## What's next

- Linking a login account to a specific `Employee` record, so an Employee-role user only sees their own timesheets rather than everyone's
- Persisted rate history for accurate historical pay recalculation (pay is currently calculated from the employee's *current* hourly rate)
- A minimal frontend for the dashboard view

## License

MIT
