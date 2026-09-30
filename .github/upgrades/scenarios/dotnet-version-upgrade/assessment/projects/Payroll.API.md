# Payroll.API\Payroll.API.csproj

[← Back to the assessment index](../../assessment.md)

## Project Info

- **Current Target Framework:** net8.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** AspNetCore
- **Dependencies**: 3
- **Dependants**: 1
- **Number of Files**: 4
- **Number of Files with Incidents**: 2
- **Lines of Code**: 386
- **Estimated LOC to modify**: 6+ (at least 1.6% of the project)

## Related Projects

**Depends on (3)** — projects this one references:

- [c:\Users\chunl\Documents\Personal\PayrollAnomalyDashboard\Payroll.Application\Payroll.Application.csproj](../projects/Payroll.Application.md)
- [c:\Users\chunl\Documents\Personal\PayrollAnomalyDashboard\Payroll.Domain\Payroll.Domain.csproj](../projects/Payroll.Domain.md)
- [c:\Users\chunl\Documents\Personal\PayrollAnomalyDashboard\Payroll.Infrastructure\Payroll.Infrastructure.csproj](../projects/Payroll.Infrastructure.md)

**Depended on by (1)** — projects that reference this one:

- [c:\Users\chunl\Documents\Personal\PayrollAnomalyDashboard\Payroll.IntegrationTests\Payroll.IntegrationTests.csproj](../projects/Payroll.IntegrationTests.md)

## Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (1)"]
        P7["<b>📦&nbsp;Payroll.IntegrationTests.csproj</b><br/><small>net8.0</small>"]
        click P7 "../projects/Payroll.IntegrationTests.md"
    end
    subgraph current["Payroll.API.csproj"]
        MAIN["<b>📦&nbsp;Payroll.API.csproj</b><br/><small>net8.0</small>"]
        click MAIN "../projects/Payroll.API.md"
    end
    subgraph downstream["Dependencies (3)"]
        P1["<b>📦&nbsp;Payroll.Domain.csproj</b><br/><small>net8.0</small>"]
        P4["<b>📦&nbsp;Payroll.Application.csproj</b><br/><small>net8.0</small>"]
        P5["<b>📦&nbsp;Payroll.Infrastructure.csproj</b><br/><small>net8.0</small>"]
        click P1 "../projects/Payroll.Domain.md"
        click P4 "../projects/Payroll.Application.md"
        click P5 "../projects/Payroll.Infrastructure.md"
    end
    P7 --> MAIN
    MAIN --> P1
    MAIN --> P4
    MAIN --> P5

```

## API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 1 | High - Require code changes |
| 🟡 Source Incompatible | 5 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 567 |  |
| ***Total APIs Analyzed*** | ***573*** |  |

## NuGet Package Issues

| Package | Current Version | Suggested Version | Severity | Issue |
| :--- | :---: | :---: | :---: | :--- |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.10 | 10.0.12 | 🟡 Potential | NuGet package upgrade is recommended |
| Microsoft.AspNetCore.OpenApi | 8.0.28 | 10.0.12 | 🟡 Potential | NuGet package upgrade is recommended |
| Microsoft.EntityFrameworkCore.Design | 8.0.10 | 10.0.12 | 🟡 Potential | NuGet package upgrade is recommended |

Every project affected by these packages, and the versions the repository settles on: [aggregate NuGet packages](../nuget/aggregate-packages.md).

