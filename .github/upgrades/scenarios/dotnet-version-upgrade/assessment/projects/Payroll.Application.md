# Payroll.Application\Payroll.Application.csproj

[← Back to the assessment index](../../assessment.md)

## Project Info

- **Current Target Framework:** net8.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 4
- **Number of Files**: 32
- **Number of Files with Incidents**: 1
- **Lines of Code**: 693
- **Estimated LOC to modify**: 0+ (at least 0.0% of the project)

## Related Projects

**Depends on (1)** — projects this one references:

- [c:\Users\chunl\Documents\Personal\PayrollAnomalyDashboard\Payroll.Domain\Payroll.Domain.csproj](../projects/Payroll.Domain.md)

**Depended on by (4)** — projects that reference this one:

- [c:\Users\chunl\Documents\Personal\PayrollAnomalyDashboard\Payroll.API\Payroll.API.csproj](../projects/Payroll.API.md)
- [c:\Users\chunl\Documents\Personal\PayrollAnomalyDashboard\Payroll.Application.Tests\Payroll.Application.Tests.csproj](../projects/Payroll.Application.Tests.md)
- [c:\Users\chunl\Documents\Personal\PayrollAnomalyDashboard\Payroll.Infrastructure\Payroll.Infrastructure.csproj](../projects/Payroll.Infrastructure.md)
- [c:\Users\chunl\Documents\Personal\PayrollAnomalyDashboard\Payroll.IntegrationTests\Payroll.IntegrationTests.csproj](../projects/Payroll.IntegrationTests.md)

## Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (4)"]
        P2["<b>📦&nbsp;Payroll.API.csproj</b><br/><small>net8.0</small>"]
        P5["<b>📦&nbsp;Payroll.Infrastructure.csproj</b><br/><small>net8.0</small>"]
        P6["<b>📦&nbsp;Payroll.Application.Tests.csproj</b><br/><small>net8.0</small>"]
        P7["<b>📦&nbsp;Payroll.IntegrationTests.csproj</b><br/><small>net8.0</small>"]
        click P2 "../projects/Payroll.API.md"
        click P5 "../projects/Payroll.Infrastructure.md"
        click P6 "../projects/Payroll.Application.Tests.md"
        click P7 "../projects/Payroll.IntegrationTests.md"
    end
    subgraph current["Payroll.Application.csproj"]
        MAIN["<b>📦&nbsp;Payroll.Application.csproj</b><br/><small>net8.0</small>"]
        click MAIN "../projects/Payroll.Application.md"
    end
    subgraph downstream["Dependencies (1)"]
        P1["<b>📦&nbsp;Payroll.Domain.csproj</b><br/><small>net8.0</small>"]
        click P1 "../projects/Payroll.Domain.md"
    end
    P2 --> MAIN
    P5 --> MAIN
    P6 --> MAIN
    P7 --> MAIN
    MAIN --> P1

```

## API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 1251 |  |
| ***Total APIs Analyzed*** | ***1251*** |  |

## NuGet Package Issues

| Package | Current Version | Suggested Version | Severity | Issue |
| :--- | :---: | :---: | :---: | :--- |
| Microsoft.Extensions.Identity.Core | 8.0.10 | 10.0.12 | 🟡 Potential | NuGet package upgrade is recommended |
| Microsoft.Extensions.Logging.Abstractions | 10.0.0 | 10.0.12 | 🟡 Potential | NuGet package upgrade is recommended |

Every project affected by these packages, and the versions the repository settles on: [aggregate NuGet packages](../nuget/aggregate-packages.md).

