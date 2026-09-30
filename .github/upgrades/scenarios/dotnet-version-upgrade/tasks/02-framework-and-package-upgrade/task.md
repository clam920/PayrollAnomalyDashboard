# 02-framework-and-package-upgrade: Retarget the complete solution to net10.0

Retarget Payroll.Domain, Payroll.Application, Payroll.Infrastructure, Payroll.API, Payroll.Domain.Tests, Payroll.Application.Tests, and Payroll.IntegrationTests from net8.0 to net10.0 in the same atomic pass. Apply the assessed package updates alongside the TFM changes, including Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.AspNetCore.OpenApi, Microsoft.EntityFrameworkCore.Design, Microsoft.Extensions.Identity.Core, Microsoft.Extensions.Logging.Abstractions, and Microsoft.AspNetCore.Mvc.Testing where applicable.

Keep project references and test references aligned across the solution. The API and infrastructure projects carry the highest compatibility concentration, while integration tests include six HttpContent behavioral-change findings and must remain part of the same upgrade operation.

## Research Findings

- Confirmed scope is the seven projects listed above; each is SDK-style and currently declares `<TargetFramework>net8.0</TargetFramework>` directly in its project file.
- Confirmed no `global.json`, `Directory.Build.props`, `Directory.Build.targets`, or `Directory.Packages.props` exists in the repository. Package versions are currently inline `PackageReference` attributes, so CPM must be introduced at the repository root before removing those inline versions.
- Confirmed project references are intact and form this dependency graph: `Payroll.Domain` -> `Payroll.Application` -> `Payroll.Infrastructure` -> `Payroll.API` -> `Payroll.IntegrationTests`; `Payroll.Domain.Tests` references Domain and `Payroll.Application.Tests` references Application.
- Confirmed the solution contains exactly the seven scoped projects and no legacy/non-SDK project requiring conversion. No `// STUB:` markers were found in affected C# files.
- Assessment consultation confirmed mandatory TFM findings for all seven projects. API has 1 binary and 5 source compatibility findings; Infrastructure has 5 binary IdentityModel findings; IntegrationTests has 6 HttpContent behavioral findings. Those API compatibility fixes are intentionally deferred to task 3 per the scenario plan.
- Assessed package actions from the aggregate report and project dependency inspection: add central versions `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.12, `Microsoft.AspNetCore.Mvc.Testing` 10.0.12, `Microsoft.AspNetCore.OpenApi` 10.0.12, `Microsoft.EntityFrameworkCore.Design` 10.0.12, `Microsoft.Extensions.Identity.Core` 10.0.12, and `Microsoft.Extensions.Logging.Abstractions` 10.0.12. `xunit` 2.5.3 is deprecated but has no assessed replacement and remains unchanged as a documented follow-up.
- Review follow-up research confirmed that `Npgsql.EntityFrameworkCore.PostgreSQL` must move to the EF Core 10-compatible major line. A live `dotnet list Payroll.Infrastructure\\Payroll.Infrastructure.csproj package --outdated --include-transitive` query against nuget.org reports stable `10.0.3` as the current version, so the central version will change from 8.0.10 to 10.0.3. The provider is used by `Payroll.Infrastructure` and the API's `UseNpgsql` configuration; no other Npgsql package changes are required for this minimal fix.
- Review follow-up research found no `SSH`, `Sftp`, `Renci`, or `Renci.SshNet` source usage under `Payroll.IntegrationTests`; the package was only added as a project reference and central pin. Both entries will be removed. OpenAPI compatibility work remains deferred to task 3.
- Other explicit packages remain at their assessed versions, including MediatR 14.2.0, FluentValidation 11.9.0, System.IdentityModel.Tokens.Jwt 8.2.0, test SDK 17.8.0, Moq 4.20.70, coverlet 6.0.0, and Testcontainers.PostgreSql 3.10.0.

## Execution and Decomposition Assessment

- Loaded the .NET version-upgrade execution guidance, common breakdown hints, and test breakdown hints. The common `multi-project-dependency-ordering` hint matches because this is a multi-project TFM/package change with a dependency chain; the test lifecycle hint also matches because three test projects depend on changed projects. No package-replacement or stub-resolution hint matches.
- The selected scenario strategy is explicitly all-at-once and requires all seven projects to change in one atomic operation. The project files are small, the package action is a single CPM conversion plus six centralized version updates, and no independent API implementation is in this task. Treat the task as one coherent atomic unit while preserving dependency order in the final validation build.

**Done when**: Every project targets net10.0, project-to-project references remain intact, all selected package versions restore under CPM, and the solution reaches compilation with only known API-fix work remaining.
