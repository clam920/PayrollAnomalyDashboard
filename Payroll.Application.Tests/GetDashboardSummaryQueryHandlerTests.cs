using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Payroll.Application.Interfaces;
using Payroll.Application.Queries;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;
using Xunit;

namespace Payroll.Application.Tests;

public class GetDashboardSummaryQueryHandlerTests
{
    private readonly Mock<IAnomalyRepository> _anomalyRepository = new();
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly GetDashboardSummaryQueryHandler _handler;

    public GetDashboardSummaryQueryHandlerTests()
    {
        _handler = new GetDashboardSummaryQueryHandler(_anomalyRepository.Object, _employeeRepository.Object);
    }

    [Fact]
    public async Task Handle_NoAnomalies_ReturnsEmptySummary()
    {
        _anomalyRepository.Setup(r => r.GetAllAsync(null)).ReturnsAsync(new List<Anomaly>());
        _employeeRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Employee>());

        var result = await _handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        Assert.Equal(0, result.TotalAnomalies);
        Assert.Empty(result.AnomaliesBySeverity);
        Assert.Empty(result.AnomaliesByType);
        Assert.Empty(result.TopFlaggedEmployees);
    }

    [Fact]
    public async Task Handle_MixedAnomalies_GroupsBySeverityAndType()
    {
        var employeeId = Guid.NewGuid();
        var anomalies = new List<Anomaly>
        {
            new(Guid.NewGuid(), employeeId, AnomalyType.ExcessiveHours, AnomalySeverity.Warning, "a"),
            new(Guid.NewGuid(), employeeId, AnomalyType.ExcessiveHours, AnomalySeverity.Critical, "b"),
            new(Guid.NewGuid(), employeeId, AnomalyType.WeekendWork, AnomalySeverity.Info, "c"),
        };
        _anomalyRepository.Setup(r => r.GetAllAsync(null)).ReturnsAsync(anomalies);
        _employeeRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Employee>());

        var result = await _handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        Assert.Equal(3, result.TotalAnomalies);
        Assert.Equal(2, result.AnomaliesByType[nameof(AnomalyType.ExcessiveHours)]);
        Assert.Equal(1, result.AnomaliesByType[nameof(AnomalyType.WeekendWork)]);
        Assert.Equal(1, result.AnomaliesBySeverity[nameof(AnomalySeverity.Warning)]);
        Assert.Equal(1, result.AnomaliesBySeverity[nameof(AnomalySeverity.Critical)]);
        Assert.Equal(1, result.AnomaliesBySeverity[nameof(AnomalySeverity.Info)]);
    }

    [Fact]
    public async Task Handle_TopFlaggedEmployees_OrdersByCountDescendingAndRespectsLimit()
    {
        var busiest = new Employee("Busy", "Bee", 20m);
        var quiet = new Employee("Quiet", "Mouse", 20m);
        var anomalies = new List<Anomaly>
        {
            new(Guid.NewGuid(), busiest.Id, AnomalyType.ExcessiveHours, AnomalySeverity.Warning, "a"),
            new(Guid.NewGuid(), busiest.Id, AnomalyType.WeekendWork, AnomalySeverity.Info, "b"),
            new(Guid.NewGuid(), quiet.Id, AnomalyType.WeekendWork, AnomalySeverity.Info, "c"),
        };
        _anomalyRepository.Setup(r => r.GetAllAsync(null)).ReturnsAsync(anomalies);
        _employeeRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Employee> { busiest, quiet });

        var result = await _handler.Handle(new GetDashboardSummaryQuery(TopEmployeeCount: 1), CancellationToken.None);

        var top = Assert.Single(result.TopFlaggedEmployees);
        Assert.Equal(busiest.Id, top.EmployeeId);
        Assert.Equal("Busy Bee", top.EmployeeName);
        Assert.Equal(2, top.AnomalyCount);
    }

    [Fact]
    public async Task Handle_AnomalyForUnknownEmployee_FallsBackToUnknownNameInsteadOfThrowing()
    {
        var orphanedEmployeeId = Guid.NewGuid();
        var anomalies = new List<Anomaly>
        {
            new(Guid.NewGuid(), orphanedEmployeeId, AnomalyType.ExcessiveHours, AnomalySeverity.Warning, "a"),
        };
        _anomalyRepository.Setup(r => r.GetAllAsync(null)).ReturnsAsync(anomalies);
        _employeeRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Employee>());

        var result = await _handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        var summary = Assert.Single(result.TopFlaggedEmployees);
        Assert.Equal("Unknown", summary.EmployeeName);
    }
}