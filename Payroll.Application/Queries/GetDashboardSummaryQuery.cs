using System;
using System.Collections.Generic;
using MediatR;

namespace Payroll.Application.Queries;

public record GetDashboardSummaryQuery(int TopEmployeeCount = 5) : IRequest<DashboardSummaryDto>;

public record DashboardSummaryDto(
    int TotalAnomalies,
    IReadOnlyDictionary<string, int> AnomaliesBySeverity,
    IReadOnlyDictionary<string, int> AnomaliesByType,
    IReadOnlyList<FlaggedEmployeeSummary> TopFlaggedEmployees);

public record FlaggedEmployeeSummary(Guid EmployeeId, string EmployeeName, int AnomalyCount);