# Payroll Anomaly Dashboard

![CI](https://github.com/clam920/PayrollAnomalyDashboard/actions/workflows/ci.yml/badge.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Tests](https://img.shields.io/badge/tests-42%20passing-brightgreen)

A backend API for payroll processing that automatically **flags anomalous timesheets** before they reach payroll — excessive hours, duplicate entries, unusually high pay, and more — with an approval workflow that requires a documented reason to override a flag.

Built to practice production-grade .NET/C# patterns: Clean Architecture, CQRS with MediatR, domain-driven design, and the cross-cutting concerns (validation, structured logging, centralized error handling) that separate a tutorial project from something closer to real engineering.

## Why this exists

Payroll errors are expensive and easy to miss — a typo'd hours field, a duplicate submission, an unapproved 18-hour shift. Most small systems either trust every submission or drown reviewers in noise. This project's core idea is a **pluggable rule-based anomaly detector** that runs automatically on every timesheet submission, paired with a review workflow that forces a human to *consciously* override a flagged item rather than silently approving it.

## Features

- **Automatic anomaly detection** on every timesheet submission — excessive hours, duplicate work dates, weekend work, and unusually high calculated pay
- **Payroll calculation** with regular/overtime (1.5×) pay split
- **Approve/reject workflow** — approving a flagged timesheet requires a non-empty override reason; rejecting a flagged one doesn't
- **Dashboard summary endpoint** — anomaly counts by severity/type and a most-flagged-employees leaderboard
- **Input validation** via FluentValidation, running as a MediatR pipeline behavior before any handler executes
- **Centralized error handling** — one middleware maps domain/validation exceptions to consistent HTTP status codes instead of per-endpoint try/catch
- **Structured logging** via Serilog, including an audit trail for flagged-timesheet overrides
- **42 unit tests** across the domain and application layers (xUnit + Moq)
- **Dockerized** with a multi-stage build; **CI** runs build + full test suite on every push via GitHub Actions

## Architecture

Clean Architecture, four layers, dependencies point inward:

```
Payroll.API              → ASP.NET Core minimal API, middleware, DI composition root
Payroll.Application      → CQRS commands/queries (MediatR), validators, interfaces
Payroll.Domain           → Entities, value objects, domain services (no external dependencies)
Payroll.Infrastructure   → EF Core + SQLite implementations of the Application interfaces
```

The **anomaly detection** and **payroll calculation** rules live in `Payroll.Domain/Services` as pure, dependency-free domain services — they take plain objects in and return plain objects out, which is what makes them fully unit-testable without a database or mocking framework.

## Tech stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 8 (minimal APIs), Swagger/OpenAPI |
| Application | MediatR (CQRS), FluentValidation |
| Data | EF Core 8, SQLite |
| Logging | Serilog (structured console logging) |
| Testing | xUnit, Moq |
| CI/CD | Docker (multi-stage build), GitHub Actions |

## API endpoints

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/employees` | Create an employee |
| `GET` | `/api/employees` | List all employees |
| `POST` | `/api/timesheets` | Submit a timesheet — runs anomaly detection, returns the flagged count |
| `GET` | `/api/timesheets?employeeId=` | List timesheets, optionally filtered by employee |
| `GET` | `/api/timesheets/{id}/pay` | Regular/overtime hours and pay for one timesheet |
| `PUT` | `/api/timesheets/{id}/approve` | Approve a timesheet (requires `overrideReason` if flagged) |
| `PUT` | `/api/timesheets/{id}/reject` | Reject a timesheet |
| `GET` | `/api/anomalies?minSeverity=` | List flagged anomalies, optionally filtered by minimum severity |
| `GET` | `/api/dashboard/summary?topEmployeeCount=` | Aggregate anomaly stats + most-flagged-employees leaderboard |

Full request/response schemas are available via Swagger UI when running in Development mode (see below).

## Getting started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- (Optional) [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Run locally
```bash
git clone https://github.com/clam920/PayrollAnomalyDashboard.git
cd PayrollAnomalyDashboard
dotnet run --project Payroll.API
```
Then open `https://localhost:{port}/swagger` (port is shown in the console output).

> The SQLite database (`payroll.db`) is created automatically on first run via `EnsureCreated()`. If you pull an update that changes an entity's shape, delete `Payroll.API/payroll.db` and re-run.

### Run with Docker
```bash
docker build -t payroll-anomaly-dashboard .
docker run -e ASPNETCORE_ENVIRONMENT=Development -p 8080:8080 payroll-anomaly-dashboard
```
Then open `http://localhost:8080/swagger`.

### Run tests
```bash
dotnet test
```
Runs both `Payroll.Domain.Tests` (pure domain logic) and `Payroll.Application.Tests` (MediatR command/query handlers, with repositories mocked via Moq).

## Project structure
```
Payroll.API/
  Middleware/              Global exception-handling middleware
  Program.cs               Composition root, DI, endpoint mapping
Payroll.Application/
  Behaviors/                MediatR validation pipeline
  Commands/                 Write operations + their validators
  Queries/                  Read operations
  Exceptions/                Application-level exception types
  Interfaces/                Repository contracts
Payroll.Domain/
  Entities/                  Employee, Timesheet, Anomaly
  Enums/                     AnomalyType, AnomalySeverity
  Services/                  Anomaly detection, payroll calculation (pure domain logic)
  ValueObjects/               PayCalculationResult
Payroll.Infrastructure/
  Repositories/                EF Core repository implementations
  data/                        AppDbContext
Payroll.Domain.Tests/          Unit tests for domain entities & services
Payroll.Application.Tests/     Unit tests for command/query handlers (Moq)
```

## Design notes

A few decisions worth calling out for anyone reviewing this code:

- **Anomaly rules are pure and testable.** `TimesheetAnomalyDetector` takes an `Employee`, a `Timesheet`, and the employee's recent history, and returns a list of `Anomaly` objects — no database, no mocking needed to test it.
- **The override rule lives in the Application layer, not the entity.** Requiring a reason to approve a flagged timesheet needs knowledge of both `Timesheet` and `Anomaly`, which a single entity shouldn't hold — that cross-aggregate rule lives in `ApproveTimesheetCommandHandler`.
- **Validation happens twice, on purpose.** FluentValidation catches malformed input at the API boundary with a clean 400; the domain entities' own constructor guard clauses are the last line of defense regardless of how they're constructed.
- **`EnsureCreated()`, not migrations.** A deliberate simplification for a portfolio-scale project — noted in code comments as a known limitation, not an oversight.

## What's next

- Authentication/authorization (kept out of scope for now)
- Persisted rate history for accurate historical pay recalculation (pay is currently calculated from the employee's *current* hourly rate)
- A minimal frontend for the dashboard view

## License

MIT
