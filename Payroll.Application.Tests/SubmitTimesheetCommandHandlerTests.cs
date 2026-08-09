using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Payroll.Application.Commands;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;
using Payroll.Domain.Services;
using Xunit;

namespace Payroll.Application.Tests;

public class SubmitTimesheetCommandHandlerTests
{
    private readonly Mock<ITimesheetRepository> _timesheetRepository = new();
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IAnomalyRepository> _anomalyRepository = new();
    private readonly Mock<IAnomalyDetectionService> _anomalyDetectionService = new();
    private readonly SubmitTimesheetCommandHandler _handler;

    public SubmitTimesheetCommandHandlerTests()
    {
        _handler = new SubmitTimesheetCommandHandler(
            _timesheetRepository.Object,
            _employeeRepository.Object,
            _anomalyRepository.Object,
            _anomalyDetectionService.Object);
    }

    [Fact]
    public async Task Handle_EmployeeDoesNotExist_ThrowsArgumentException()
    {
        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Employee?)null);

        var command = new SubmitTimesheetCommand(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), 8m);

        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
        _timesheetRepository.Verify(r => r.AddAsync(It.IsAny<Timesheet>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InactiveEmployee_ThrowsArgumentException()
    {
        var employee = new Employee("Jane", "Doe", 25m);
        employee.Terminate();
        _employeeRepository.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync(employee);

        var command = new SubmitTimesheetCommand(employee.Id, DateTime.UtcNow.AddDays(-1), 8m);

        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
        _timesheetRepository.Verify(r => r.AddAsync(It.IsAny<Timesheet>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoAnomaliesDetected_ReturnsZeroAnomalyCountAndDoesNotTouchAnomalyRepository()
    {
        var employee = new Employee("Jane", "Doe", 25m);
        _employeeRepository.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync(employee);
        _timesheetRepository.Setup(r => r.GetRecentByEmployeeIdAsync(employee.Id, It.IsAny<int>()))
            .ReturnsAsync(new List<Timesheet>());
        _anomalyDetectionService
            .Setup(s => s.Detect(It.IsAny<Employee>(), It.IsAny<Timesheet>(), It.IsAny<IEnumerable<Timesheet>>()))
            .Returns(new List<Anomaly>());

        var command = new SubmitTimesheetCommand(employee.Id, DateTime.UtcNow.AddDays(-1), 8m);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(0, result.AnomalyCount);
        _anomalyRepository.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Anomaly>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AnomaliesDetected_PersistsThemAndReturnsMatchingCount()
    {
        var employee = new Employee("Jane", "Doe", 25m);
        _employeeRepository.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync(employee);
        _timesheetRepository.Setup(r => r.GetRecentByEmployeeIdAsync(employee.Id, It.IsAny<int>()))
            .ReturnsAsync(new List<Timesheet>());

        var detectedAnomalies = new List<Anomaly>();
        _anomalyDetectionService
            .Setup(s => s.Detect(It.IsAny<Employee>(), It.IsAny<Timesheet>(), It.IsAny<IEnumerable<Timesheet>>()))
            .Returns((Employee _, Timesheet t, IEnumerable<Timesheet> _) =>
            {
                var anomaly = new Anomaly(t.Id, employee.Id, AnomalyType.ExcessiveHours, AnomalySeverity.Warning, "test anomaly");
                detectedAnomalies.Add(anomaly);
                return detectedAnomalies;
            });

        var command = new SubmitTimesheetCommand(employee.Id, DateTime.UtcNow.AddDays(-1), 13m);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(1, result.AnomalyCount);
        _anomalyRepository.Verify(r => r.AddRangeAsync(It.Is<IEnumerable<Anomaly>>(a => a == detectedAnomalies)), Times.Once);
        _anomalyRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}