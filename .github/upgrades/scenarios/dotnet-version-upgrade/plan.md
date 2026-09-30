# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade all PayrollAnomalyDashboard projects from net8.0 to net10.0.
**Scope**: Seven-project, fully SDK-style solution with application, class library, and test projects; 198 assessed package entries and 35 total issues.

### Selected Strategy
**All-At-Once** — All projects upgraded simultaneously in a single operation.
**Rationale**: Seven projects are already on modern .NET, the dependency graph is at most three tiers deep, and the assessment shows a contained upgrade with 17+ estimated lines of code impact.

The solution is handled as one atomic upgrade. Tasks are grouped by concern for execution clarity, but no dependency-tier or phased rollout is introduced.

**Projects**: Payroll.Domain, Payroll.Application, Payroll.Infrastructure, Payroll.API, Payroll.Domain.Tests, Payroll.Application.Tests, and Payroll.IntegrationTests.

## Upgrade Options

| Option | Selected | Why |
|--------|----------|-----|
| Upgrade Strategy | All-at-Once | Seven modern SDK-style projects with a three-tier dependency graph and a contained net8.0-to-net10.0 change favor one atomic pass. |
| Package Management | Central Package Management | All projects are SDK-style, share the modern .NET ecosystem, and the seven-project solution benefits from centrally aligned package versions. |
| Unsupported API Handling | Fix Inline | The assessment identifies 11 binary or source-incompatible API findings, concentrated in JWT and configuration code, so fixes should be completed during the upgrade. |
| Test Coverage | Skip | The confirmed choice omits generated pre-upgrade baseline work; existing tests remain part of final validation. |

## Tasks

### 01-prerequisites-and-package-management: Verify tooling and establish central package management

Confirm the .NET 10 SDK and any global.json constraints, then establish Central Package Management for the seven SDK-style projects. Consolidate package versions without changing application behavior, retaining compatible versions and recording the assessed upgrade targets, including the ASP.NET Core, OpenAPI, EF Core design, Identity Core, and logging package updates.

The package inventory contains 198 entries, with six recommended package upgrades and a deprecated xunit package finding. Preserve the assessment's package compatibility decisions while ensuring project files no longer carry conflicting version declarations.

**Done when**: The .NET 10 toolchain is compatible, Directory.Packages.props centrally represents the solution's package versions, all project references resolve through CPM, and restore succeeds without package-version conflicts.

---

### 02-framework-and-package-upgrade: Retarget the complete solution to net10.0

Retarget Payroll.Domain, Payroll.Application, Payroll.Infrastructure, Payroll.API, Payroll.Domain.Tests, Payroll.Application.Tests, and Payroll.IntegrationTests from net8.0 to net10.0 in the same atomic pass. Apply the assessed package updates alongside the TFM changes, including Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.AspNetCore.OpenApi, Microsoft.EntityFrameworkCore.Design, Microsoft.Extensions.Identity.Core, Microsoft.Extensions.Logging.Abstractions, and Microsoft.AspNetCore.Mvc.Testing where applicable.

Keep project references and test references aligned across the solution. The API and infrastructure projects carry the highest compatibility concentration, while integration tests include six HttpContent behavioral-change findings and must remain part of the same upgrade operation.

**Done when**: Every project targets net10.0, project-to-project references remain intact, all selected package versions restore under CPM, and the solution reaches compilation with only known API-fix work remaining.

---

### 03-inline-api-compatibility-fixes: Resolve net10.0 API and behavior changes

Apply all required compatibility fixes inline across Payroll.API, Payroll.Infrastructure, and Payroll.IntegrationTests. Address the assessed JWT bearer registration and token-generation incompatibilities, configuration binder usage, JwtBearer options and scheme APIs, and the HttpContent behavioral changes; preserve authentication, token, configuration, persistence, and endpoint behavior while updating affected call sites.

Use the assessment's detailed API findings as the research starting point and verify the related existing unit and integration tests continue to express the intended contracts. No stubs or deferred API-resolution subtasks are planned because Fix Inline was confirmed.

**Done when**: All flagged binary and source incompatibilities are resolved without compatibility stubs, affected behavioral call sites compile against net10.0 packages, and existing focused tests pass for the changed authentication, infrastructure, and endpoint paths.

---

### 04-final-solution-validation: Build and test the upgraded solution

Run the complete solution validation after the atomic upgrade: restore, build all seven projects, execute the existing unit and integration test projects, and verify that the API test host and persistence-backed integration paths still work. Check that CPM produces a consistent dependency graph and that no package downgrade, unresolved binding, or target-framework mismatch remains.

Capture any remaining non-blocking package recommendation, including the deprecated xunit package, as documented follow-up rather than silently changing scope. Test Coverage is skipped, so do not create or execute a generated baseline workflow.

**Done when**: The full solution builds with zero errors, all existing tests pass, package restore reports no conflicts or vulnerabilities identified by the assessment, every project remains on net10.0, and any deferred recommendation is documented.
