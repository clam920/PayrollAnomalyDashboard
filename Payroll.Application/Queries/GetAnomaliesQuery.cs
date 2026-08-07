using System;
using System.Collections.Generic;
using MediatR;
using Payroll.Domain.Enums;

namespace Payroll.Application.Queries;

// MinSeverity binds directly from the query string as an enum (ASP.NET Core
// minimal APIs handle "?minSeverity=Warning" out of the box), so no manual
// string-to-enum parsing is needed in the API layer.
public record GetAnomaliesQuery(AnomalySeverity? MinSeverity = null) : IRequest<List<AnomalyDto>>;

public record AnomalyDto(
    Guid Id,
    Guid TimesheetId,
    Guid EmployeeId,
    string Type,
    string Severity,
    string Description,
    DateTime DetectedAtUtc);