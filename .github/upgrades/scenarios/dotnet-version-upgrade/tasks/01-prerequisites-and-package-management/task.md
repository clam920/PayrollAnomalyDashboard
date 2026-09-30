# 01-prerequisites-and-package-management: Verify tooling and establish central package management

Confirm the .NET 10 SDK and any global.json constraints, then establish Central Package Management for the seven SDK-style projects. Consolidate package versions without changing application behavior, retaining compatible versions and recording the assessed upgrade targets, including the ASP.NET Core, OpenAPI, EF Core design, Identity Core, and logging package updates.

The package inventory contains 198 entries, with six recommended package upgrades and a deprecated xunit package finding. Preserve the assessment's package compatibility decisions while ensuring project files no longer carry conflicting version declarations.

**Done when**: The .NET 10 toolchain is compatible, Directory.Packages.props centrally represents the solution's package versions, all project references resolve through CPM, and restore succeeds without package-version conflicts.

## Research Findings

- Scope confirmed from `PayrollAnomalyDashboard.sln`: seven SDK-style projects, all currently targeting `net8.0`.
- Project dependency tiers are `Payroll.Domain`; `Payroll.Application` and `Payroll.Infrastructure`; `Payroll.API`; then `Payroll.Application.Tests`, `Payroll.Domain.Tests`, and `Payroll.IntegrationTests`.
- The required pre-change solution build succeeded with `dotnet build PayrollAnomalyDashboard.sln --no-restore` on 2026-09-29, with no reported warnings or errors.
- .NET SDK `10.0.401` and .NET 10 runtime `10.0.12` are installed. No `global.json` exists, so there is no SDK pin to update or validate.
- No `Directory.Packages.props` or other `Directory*.props` file exists. All package versions are currently declared inline in the seven project files; `Payroll.Domain.csproj` has no package references.
- No `// STUB:` markers were found in the affected source trees.

### Package Actions

- Centralize every direct `PackageReference` version without changing compatible versions.
- Apply the assessed .NET 10 package targets: `Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.AspNetCore.OpenApi`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.Extensions.Identity.Core`, and `Microsoft.Extensions.Logging.Abstractions` to `10.0.12`.
- Preserve the assessed compatible versions, including `Microsoft.Extensions.Logging.Abstractions`'s current `10.0.0` only until the assessed target is applied, and retain `xunit` `2.5.3` while documenting its deprecated-package finding for follow-up.
- Preserve `PrivateAssets` and `IncludeAssets` metadata on the two EF Core Design references.

### Decomposition Assessment

- Evaluated `execution.md`, `breakdown-hints/common.md`, and `breakdown-hints/test.md`.
- The common multi-project dependency-ordering hint applies because this task changes package management across seven projects with a three-tier dependency chain.
- The test-project lifecycle hint applies because three test projects depend on projects in the same scope. No stub-resolution or large incompatible-package-replacement hint applies.
- Escalate to `TaskBreaker` before source edits to determine dependency-tier subtasks and validation boundaries.
