# Payroll.Infrastructure\Payroll.Infrastructure.csproj

[← Back to the assessment index](../../assessment.md)

## Project Info

- **Current Target Framework:** net8.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 2
- **Dependants**: 2
- **Number of Files**: 10
- **Number of Files with Incidents**: 2
- **Lines of Code**: 533
- **Estimated LOC to modify**: 5+ (at least 0.9% of the project)

## Related Projects

**Depends on (2)** — projects this one references:

- [c:\Users\chunl\Documents\Personal\PayrollAnomalyDashboard\Payroll.Application\Payroll.Application.csproj](../projects/Payroll.Application.md)
- [c:\Users\chunl\Documents\Personal\PayrollAnomalyDashboard\Payroll.Domain\Payroll.Domain.csproj](../projects/Payroll.Domain.md)

**Depended on by (2)** — projects that reference this one:

- [c:\Users\chunl\Documents\Personal\PayrollAnomalyDashboard\Payroll.API\Payroll.API.csproj](../projects/Payroll.API.md)
- [c:\Users\chunl\Documents\Personal\PayrollAnomalyDashboard\Payroll.IntegrationTests\Payroll.IntegrationTests.csproj](../projects/Payroll.IntegrationTests.md)

## Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (2)"]
        P2["<b>📦&nbsp;Payroll.API.csproj</b><br/><small>net8.0</small>"]
        P7["<b>📦&nbsp;Payroll.IntegrationTests.csproj</b><br/><small>net8.0</small>"]
        click P2 "../projects/Payroll.API.md"
        click P7 "../projects/Payroll.IntegrationTests.md"
    end
    subgraph current["Payroll.Infrastructure.csproj"]
        MAIN["<b>📦&nbsp;Payroll.Infrastructure.csproj</b><br/><small>net8.0</small>"]
        click MAIN "../projects/Payroll.Infrastructure.md"
    end
    subgraph downstream["Dependencies (2)"]
        P4["<b>📦&nbsp;Payroll.Application.csproj</b><br/><small>net8.0</small>"]
        P1["<b>📦&nbsp;Payroll.Domain.csproj</b><br/><small>net8.0</small>"]
        click P4 "../projects/Payroll.Application.md"
        click P1 "../projects/Payroll.Domain.md"
    end
    P2 --> MAIN
    P7 --> MAIN
    MAIN --> P4
    MAIN --> P1

```

## API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 5 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 735 |  |
| ***Total APIs Analyzed*** | ***740*** |  |

## NuGet Package Issues

| Package | Current Version | Suggested Version | Severity | Issue |
| :--- | :---: | :---: | :---: | :--- |
| Microsoft.EntityFrameworkCore.Design | 8.0.10 | 10.0.12 | 🟡 Potential | NuGet package upgrade is recommended |
| Microsoft.Extensions.Identity.Core | 8.0.10 | 10.0.12 | 🟡 Potential | NuGet package upgrade is recommended |

Every project affected by these packages, and the versions the repository settles on: [aggregate NuGet packages](../nuget/aggregate-packages.md).

## Project Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |
| IdentityModel & Claims-based Security | 5 | 100.0% | Windows Identity Foundation (WIF), SAML, and claims-based authentication APIs that have been replaced by modern identity libraries. WIF was the original identity framework for .NET Framework. Migrate to Microsoft.IdentityModel.* packages (modern identity stack). |

