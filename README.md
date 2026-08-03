# Payroll Anomaly Detection & Ledger API

An enterprise-grade .NET 8 Web API designed to securely process massive payroll runs, manage employee timesheets, and enforce strict financial domain rules. 

This project is built using **Clean Architecture** and **Domain-Driven Design (DDD)** to ensure the core business logic remains entirely decoupled from the database and web framework.

## 🏗️ Architecture & Tech Stack

* **Platform:** .NET 8.0, C# 12
* **Architecture:** Clean Architecture, CQRS (Command Query Responsibility Segregation)
* **API:** ASP.NET Core Minimal APIs
* **Mediation:** MediatR
* **Data Access:** Entity Framework Core (SQLite for local development)
* **Testing:** xUnit

## 📂 Project Structure

The solution enforces one-way dependencies to maintain a highly testable and robust codebase:

1. **`Payroll.Domain` (Core)**
   * Contains immutable business entities (`Employee`, `Timesheet`).
   * Enforces strict validation and state machine transitions (e.g., Draft -> Submitted -> Approved).
   * *Has no external dependencies.*

2. **`Payroll.Application` (Use Cases)**
   * Implements the CQRS pattern using MediatR.
   * Contains `SubmitTimesheetCommand` and abstracts database operations via `ITimesheetRepository`.

3. **`Payroll.Infrastructure` (Data Layer)**
   * Implements the repository interfaces.
   * Manages the EF Core `AppDbContext` and database configurations.

4. **`Payroll.API` (Presentation)**
   * The entry point of the application.
   * Handles Dependency Injection (DI) and HTTP routing.

## 🚀 Getting Started

### Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Running the API
1. Clone the repository.
2. Navigate to the root directory.
3. Run the application:
   ```bash
   dotnet run --project Payroll.API/Payroll.API.csproj