# Projects and dependencies analysis

This document provides a comprehensive overview of the projects and their dependencies in the context of upgrading to .NETCoreApp,Version=v10.0.

Detailed findings live alongside this file in `assessment/`. This page is the index: read it first, then open only the documents you need.

## Table of Contents

- [Executive Summary](#executive-summary)
  - [Highlevel Metrics](#highlevel-metrics)
  - [Projects Compatibility](#projects-compatibility)
  - [Package Compatibility](#package-compatibility)
  - [API Compatibility](#api-compatibility)
- [Top API Migration Challenges](#top-api-migration-challenges)
  - [Technologies and Features](#technologies-and-features)
  - [Most Frequent API Issues](#most-frequent-api-issues)
- [Detailed Reports](#detailed-reports)
  - [Projects Relationship Graph](assessment/project-graph.md)
  - [Aggregate NuGet packages details](assessment/nuget/aggregate-packages.md)
  - [Most Frequent API Issues (complete list)](assessment/api-issues/most-frequent-api-issues.md)
  - [Project Details](#project-details)

## Executive Summary

### Highlevel Metrics

| Metric | Count | Status |
| :--- | :---: | :--- |
| Total Projects | 7 | All require upgrade |
| Total NuGet Packages | 198 | 7 need upgrade |
| Total Code Files | 68 |  |
| Total Code Files with Incidents | 12 |  |
| Total Lines of Code | 3024 |  |
| Total Number of Issues | 35 |  |
| Proposed Target Framework | net10.0 |  |
| Estimated LOC to modify | 17+ | at least 0.6% of codebase |

### Projects Compatibility

| Project | Target Framework | Difficulty | Test Coverage | Package Issues | API Issues | Binding Issues | Est. LOC Impact | Description |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| [Payroll.API\Payroll.API.csproj](assessment/projects/Payroll.API.md) | net8.0 | 🟢 Low | 🧪 Recommended | 3 | 6 | 0 | 6+ | AspNetCore, Sdk Style = True |
| [Payroll.Application.Tests\Payroll.Application.Tests.csproj](assessment/projects/Payroll.Application.Tests.md) | net8.0 | 🟢 Low | — | 1 | 0 | 0 |  | DotNetCoreApp, Sdk Style = True |
| [Payroll.Application\Payroll.Application.csproj](assessment/projects/Payroll.Application.md) | net8.0 | 🟢 Low | — | 2 | 0 | 0 |  | ClassLibrary, Sdk Style = True |
| [Payroll.Domain.Tests\Payroll.Domain.Tests.csproj](assessment/projects/Payroll.Domain.Tests.md) | net8.0 | 🟢 Low | — | 1 | 0 | 0 |  | DotNetCoreApp, Sdk Style = True |
| [Payroll.Domain\Payroll.Domain.csproj](assessment/projects/Payroll.Domain.md) | net8.0 | 🟢 Low | — | 0 | 0 | 0 |  | ClassLibrary, Sdk Style = True |
| [Payroll.Infrastructure\Payroll.Infrastructure.csproj](assessment/projects/Payroll.Infrastructure.md) | net8.0 | 🟢 Low | 🧪 Recommended | 2 | 5 | 0 | 5+ | ClassLibrary, Sdk Style = True |
| [Payroll.IntegrationTests\Payroll.IntegrationTests.csproj](assessment/projects/Payroll.IntegrationTests.md) | net8.0 | 🟢 Low | — | 2 | 6 | 0 | 6+ | DotNetCoreApp, Sdk Style = True |

🧪 **Test Coverage** — projects risky enough to add behavior-locking tests before upgrading, to catch regressions the upgrade may introduce. Requires the **dotnet-test** plugin.

### Package Compatibility

| Status | Count | Percentage |
| :--- | :---: | :---: |
| ✅ Compatible | 191 | 96.5% |
| ⚠️ Incompatible | 1 | 0.5% |
| 🔄 Upgrade Recommended | 6 | 3.0% |
| ***Total NuGet Packages*** | ***198*** | ***100%*** |

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 6 | High - Require code changes |
| 🟡 Source Incompatible | 5 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 6 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 4867 |  |
| ***Total APIs Analyzed*** | ***4884*** |  |

## Top API Migration Challenges

### Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |
| IdentityModel & Claims-based Security | 5 | 29.4% | Windows Identity Foundation (WIF), SAML, and claims-based authentication APIs that have been replaced by modern identity libraries. WIF was the original identity framework for .NET Framework. Migrate to Microsoft.IdentityModel.* packages (modern identity stack). |

### Most Frequent API Issues

| API | Count | Percentage | Category |
| :--- | :---: | :---: | :--- |
| T:System.Net.Http.HttpContent | 6 | 35.3% | Behavioral Change |
| F:Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme | 1 | 5.9% | Source Incompatible |
| M:Microsoft.Extensions.Configuration.ConfigurationBinder.Get''1(Microsoft.Extensions.Configuration.IConfiguration) | 1 | 5.9% | Binary Incompatible |
| M:Microsoft.Extensions.DependencyInjection.JwtBearerExtensions.AddJwtBearer(Microsoft.AspNetCore.Authentication.AuthenticationBuilder,System.Action{Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions}) | 1 | 5.9% | Source Incompatible |
| M:System.IdentityModel.Tokens.Jwt.JwtSecurityToken.#ctor(System.String,System.String,System.Collections.Generic.IEnumerable{System.Security.Claims.Claim},System.Nullable{System.DateTime},System.Nullable{System.DateTime},Microsoft.IdentityModel.Tokens.SigningCredentials) | 1 | 5.9% | Binary Incompatible |
| M:System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.#ctor | 1 | 5.9% | Binary Incompatible |
| M:System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.WriteToken(Microsoft.IdentityModel.Tokens.SecurityToken) | 1 | 5.9% | Binary Incompatible |
| P:Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions.TokenValidationParameters | 1 | 5.9% | Source Incompatible |
| T:Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults | 1 | 5.9% | Source Incompatible |
| T:Microsoft.Extensions.DependencyInjection.JwtBearerExtensions | 1 | 5.9% | Source Incompatible |

The table above is the top 10. See [the complete list](assessment/api-issues/most-frequent-api-issues.md) for every affected API.

## Detailed Reports

- [Projects Relationship Graph](assessment/project-graph.md)
- [Aggregate NuGet packages details](assessment/nuget/aggregate-packages.md)
- [Most Frequent API Issues (complete list)](assessment/api-issues/most-frequent-api-issues.md)

### Project Details

- [Payroll.API\Payroll.API.csproj](assessment/projects/Payroll.API.md)
- [Payroll.Application.Tests\Payroll.Application.Tests.csproj](assessment/projects/Payroll.Application.Tests.md)
- [Payroll.Application\Payroll.Application.csproj](assessment/projects/Payroll.Application.md)
- [Payroll.Domain.Tests\Payroll.Domain.Tests.csproj](assessment/projects/Payroll.Domain.Tests.md)
- [Payroll.Domain\Payroll.Domain.csproj](assessment/projects/Payroll.Domain.md)
- [Payroll.Infrastructure\Payroll.Infrastructure.csproj](assessment/projects/Payroll.Infrastructure.md)
- [Payroll.IntegrationTests\Payroll.IntegrationTests.csproj](assessment/projects/Payroll.IntegrationTests.md)


