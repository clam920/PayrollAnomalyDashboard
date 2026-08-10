using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Payroll.Application.Interfaces;

namespace Payroll.Application.Queries;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IAnomalyRepository _anomalyRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public GetDashboardSummaryQueryHandler(IAnomalyRepository anomalyRepository, IEmployeeRepository employeeRepository)
    {
        _anomalyRepository = anomalyRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var anomalies = await _anomalyRepository.GetAllAsync();

        var bySeverity = anomalies
            .GroupBy(a => a.Severity.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var byType = anomalies
            .GroupBy(a => a.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var employees = await _employeeRepository.GetAllAsync();
        var employeeNames = employees.ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}");

        var topFlaggedEmployees = anomalies
            .GroupBy(a => a.EmployeeId)
            .Select(g => new FlaggedEmployeeSummary(
                g.Key,
                employeeNames.TryGetValue(g.Key, out var name) ? name : "Unknown",
                g.Count()))
            .OrderByDescending(summary => summary.AnomalyCount)
            .Take(request.TopEmployeeCount)
            .ToList();

        return new DashboardSummaryDto(anomalies.Count, bySeverity, byType, topFlaggedEmployees);
    }
}