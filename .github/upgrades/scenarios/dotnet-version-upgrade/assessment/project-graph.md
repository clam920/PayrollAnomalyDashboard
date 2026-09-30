# Projects Relationship Graph

[← Back to the assessment index](../assessment.md)

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart LR
    P1["<b>📦&nbsp;Payroll.Domain.csproj</b><br/><small>net8.0</small>"]
    P2["<b>📦&nbsp;Payroll.API.csproj</b><br/><small>net8.0</small>"]
    P3["<b>📦&nbsp;Payroll.Domain.Tests.csproj</b><br/><small>net8.0</small>"]
    P4["<b>📦&nbsp;Payroll.Application.csproj</b><br/><small>net8.0</small>"]
    P5["<b>📦&nbsp;Payroll.Infrastructure.csproj</b><br/><small>net8.0</small>"]
    P6["<b>📦&nbsp;Payroll.Application.Tests.csproj</b><br/><small>net8.0</small>"]
    P7["<b>📦&nbsp;Payroll.IntegrationTests.csproj</b><br/><small>net8.0</small>"]
    P2 --> P1
    P2 --> P4
    P2 --> P5
    P3 --> P1
    P4 --> P1
    P5 --> P4
    P5 --> P1
    P6 --> P4
    P6 --> P1
    P7 --> P2
    P7 --> P4
    P7 --> P1
    P7 --> P5
    click P1 "projects/Payroll.Domain.md"
    click P2 "projects/Payroll.API.md"
    click P3 "projects/Payroll.Domain.Tests.md"
    click P4 "projects/Payroll.Application.md"
    click P5 "projects/Payroll.Infrastructure.md"
    click P6 "projects/Payroll.Application.Tests.md"
    click P7 "projects/Payroll.IntegrationTests.md"

```

