# Payroll Anomaly Dashboard

This repository contains a .NET 8 sample solution for payroll processing built with Clean Architecture and Domain-Driven Design. The current implementation focuses on a timesheet submission workflow, business-rule validation, and a simple API for creating payroll records.

## Overview

The solution demonstrates a layered architecture that keeps business rules separate from infrastructure and presentation concerns. It includes:

- a domain model for employees and timesheets
- validation rules for hours worked and work dates
- a state machine for timesheet status transitions such as Draft, Submitted, Approved, and Rejected
- a CQRS-style command flow using MediatR
- an ASP.NET Core Web API with Swagger and SQLite for local development

## Project Structure

- Payroll.Domain: domain entities, value rules, and business behavior
- Payroll.Application: commands, handlers, and repository abstractions
- Payroll.Infrastructure: EF Core context, repository implementation, and data access
- Payroll.API: API entry point, dependency injection, and HTTP endpoints
- Payroll.Domain.Tests: xUnit tests covering the domain model behavior

## Getting Started

### Prerequisites

- .NET 8 SDK

### Build the solution

Run the following command from the repository root:

```bash
dotnet build PayrollAnomalyDashboard.sln
```

### Run the API

1. Restore dependencies:
   ```bash
   dotnet restore
   ```
2. Start the API:
   ```bash
   dotnet run --project Payroll.API/Payroll.API.csproj
   ```
3. Open the Swagger UI in your browser. The local URLs are typically:
   - http://localhost:5079/swagger
   - or https://localhost:7273/swagger

### Example API Request

Create a new timesheet by sending a POST request to /api/timesheets with a payload like:

```json
{
  "employeeId": "00000000-0000-0000-0000-000000000000",
  "workDate": "2026-08-02T00:00:00Z",
  "hoursWorked": 8
}
```

The API returns a 201 Created response when the request is accepted and a 400 Bad Request response when the domain rules reject the data.

## Testing

Run the test suite with:

```bash
dotnet test
```

## Notes

The API creates a local SQLite database automatically on startup for development convenience. This project is intended as a portfolio-style example of a modular .NET architecture rather than a full production payroll platform.