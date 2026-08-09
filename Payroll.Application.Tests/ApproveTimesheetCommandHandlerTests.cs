using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Payroll.Application.Commands;
using Payroll.Application.Exceptions;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;
using Xunit;

namespace Payroll.Application.Tests;

public class ApproveTimesheetCommandHandlerTests
{
    private readonly Mock<ITimesheetRepository> _timesheetRepository = new();
    private readonly Mock<IAnomalyRepository> _anomalyRepository = new();
    private readonly ApproveTimesheetCommandHandler _handler;

    public ApproveTimesheetCommandHandlerTests()
    {
        _handler = new ApproveTimesheetCommandHandler(
            _timesheetRepository.Object,
            _anomalyRepository.Object,
            NullLogger<ApproveTimesheetCommandHandler>.Instance);
    }

    private static Timesheet CreateSubmittedTimesheet()
    {
        var timesheet = new Timesheet(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), 8m);
        timesheet.Submit();
        return timesheet;
    }

    [Fact]
    public async Task Handle_TimesheetNotFound_ThrowsNotFoundException()
    {
        _timesheetRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Timesheet?)null);

        var command = new ApproveTimesheetCommand(Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NoAnomalies_ApprovesWithoutRequiringAReason()
    {
        var timesheet = CreateSubmittedTimesheet();
        _timesheetRepository.Setup(r => r.GetByIdAsync(timesheet.Id)).ReturnsAsync(timesheet);
        _anomalyRepository.Setup(r => r.GetByTimesheetIdAsync(timesheet.Id)).ReturnsAsync(new List<Anomaly>());

        var command = new ApproveTimesheetCommand(timesheet.Id);

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(TimesheetStatus.Approved, timesheet.Status);
        _timesheetRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_HasAnomaliesAndNoOverrideReason_ThrowsInvalidOperationExceptionAndDoesNotApprove()
    {
        var timesheet = CreateSubmittedTimesheet();
        var flaggedAnomalies = new List<Anomaly>
        {
            new(timesheet.Id, timesheet.EmployeeId, AnomalyType.ExcessiveHours, AnomalySeverity.Warning, "flagged")
        };
        _timesheetRepository.Setup(r => r.GetByIdAsync(timesheet.Id)).ReturnsAsync(timesheet);
        _anomalyRepository.Setup(r => r.GetByTimesheetIdAsync(timesheet.Id)).ReturnsAsync(flaggedAnomalies);

        // No OverrideReason supplied - this is the core rule under test.
        var command = new ApproveTimesheetCommand(timesheet.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));

        // The timesheet must be left untouched - still Submitted, not silently approved.
        Assert.Equal(TimesheetStatus.Submitted, timesheet.Status);
        _timesheetRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Handle_HasAnomaliesAndBlankOverrideReason_ThrowsInvalidOperationException(string? blankReason)
    {
        var timesheet = CreateSubmittedTimesheet();
        var flaggedAnomalies = new List<Anomaly>
        {
            new(timesheet.Id, timesheet.EmployeeId, AnomalyType.ExcessiveHours, AnomalySeverity.Warning, "flagged")
        };
        _timesheetRepository.Setup(r => r.GetByIdAsync(timesheet.Id)).ReturnsAsync(timesheet);
        _anomalyRepository.Setup(r => r.GetByTimesheetIdAsync(timesheet.Id)).ReturnsAsync(flaggedAnomalies);

        var command = new ApproveTimesheetCommand(timesheet.Id, blankReason);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_HasAnomaliesWithOverrideReason_ApprovesAndStoresTheReason()
    {
        var timesheet = CreateSubmittedTimesheet();
        var flaggedAnomalies = new List<Anomaly>
        {
            new(timesheet.Id, timesheet.EmployeeId, AnomalyType.ExcessiveHours, AnomalySeverity.Warning, "flagged")
        };
        _timesheetRepository.Setup(r => r.GetByIdAsync(timesheet.Id)).ReturnsAsync(timesheet);
        _anomalyRepository.Setup(r => r.GetByTimesheetIdAsync(timesheet.Id)).ReturnsAsync(flaggedAnomalies);

        var command = new ApproveTimesheetCommand(timesheet.Id, "Confirmed with employee - legitimate double shift.");

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(TimesheetStatus.Approved, timesheet.Status);
        Assert.Equal("Confirmed with employee - legitimate double shift.", timesheet.ReviewNote);
        _timesheetRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}