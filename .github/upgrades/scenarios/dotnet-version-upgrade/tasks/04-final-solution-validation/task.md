# 04-final-solution-validation: Build and test the upgraded solution

Run the complete solution validation after the atomic upgrade: restore, build all seven projects, execute the existing unit and integration test projects, and verify that the API test host and persistence-backed integration paths still work. Check that CPM produces a consistent dependency graph and that no package downgrade, unresolved binding, or target-framework mismatch remains.

Capture any remaining non-blocking package recommendation, including the deprecated xunit package, as documented follow-up rather than silently changing scope. Test Coverage is skipped, so do not create or execute a generated baseline workflow.

**Done when**: The full solution builds with zero errors, all existing tests pass, package restore reports no conflicts or vulnerabilities identified by the assessment, every project remains on net10.0, and any deferred recommendation is documented.

## Research Findings

- Confirmed solution scope is the seven projects listed in `PayrollAnomalyDashboard.sln`: `Payroll.Domain`, `Payroll.Application`, `Payroll.Infrastructure`, `Payroll.API`, `Payroll.Domain.Tests`, `Payroll.Application.Tests`, and `Payroll.IntegrationTests`.
- All seven project files are SDK-style and currently declare `net10.0`. The dependency chain is Domain -> Application -> Infrastructure -> API, with the application and domain unit-test projects referencing their respective layers and the integration-test project referencing the API stack.
- Central Package Management is enabled in `Directory.Packages.props`; all project `PackageReference` items omit versions. Central versions include the .NET 10.0.12 ASP.NET Core/EF Core packages, `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3, and test packages including `xunit` 2.5.3 and `xunit.runner.visualstudio` 2.5.3.
- Assessment follow-up to verify during validation: `xunit` 2.5.3 is deprecated but optional/non-blocking. The assessment recommended the .NET 10 package updates already present; validation must confirm no restore downgrade/conflict or unresolved target-framework issue remains.
- Integration validation must cover the `WebApplicationFactory` API host and Testcontainers PostgreSQL persistence path. No `// STUB:` markers were found in C# source files.
- Applicable execution guidance evaluated: `execution.md`, `breakdown-hints/common.md`, and `breakdown-hints/test.md`. The task remains atomic because it is one final gate over the already-upgraded solution; no source or project-file edits are planned.
