using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Payroll.Application.Commands;
using Payroll.Application.Exceptions;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Xunit;

namespace Payroll.Application.Tests;

public class RejectTimesheetCommandHandlerTests
{
    private readonly Mock<ITimesheetRepository> _timesheetRepository = new();
    private readonly RejectTimesheetCommandHandler _handler;

    public RejectTimesheetCommandHandlerTests()
    {
        _handler = new RejectTimesheetCommandHandler(_timesheetRepository.Object);
    }

    [Fact]
    public async Task Handle_TimesheetNotFound_ThrowsNotFoundException()
    {
        _timesheetRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Timesheet?)null);

        var command = new RejectTimesheetCommand(Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SubmittedTimesheetNoReason_RejectsWithoutRequiringOne()
    {
        var timesheet = new Timesheet(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), 8m);
        timesheet.Submit();
        _timesheetRepository.Setup(r => r.GetByIdAsync(timesheet.Id)).ReturnsAsync(timesheet);

        var command = new RejectTimesheetCommand(timesheet.Id);

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(TimesheetStatus.Rejected, timesheet.Status);
        Assert.Null(timesheet.ReviewNote);
        _timesheetRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_SubmittedTimesheetWithReason_RejectsAndStoresTheReason()
    {
        var timesheet = new Timesheet(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), 8m);
        timesheet.Submit();
        _timesheetRepository.Setup(r => r.GetByIdAsync(timesheet.Id)).ReturnsAsync(timesheet);

        var command = new RejectTimesheetCommand(timesheet.Id, "Hours don't match the sign-in log.");

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(TimesheetStatus.Rejected, timesheet.Status);
        Assert.Equal("Hours don't match the sign-in log.", timesheet.ReviewNote);
    }

    [Fact]
    public async Task Handle_AlreadyApprovedTimesheet_ThrowsInvalidOperationException()
    {
        var timesheet = new Timesheet(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), 8m);
        timesheet.Submit();
        timesheet.Approve();
        _timesheetRepository.Setup(r => r.GetByIdAsync(timesheet.Id)).ReturnsAsync(timesheet);

        var command = new RejectTimesheetCommand(timesheet.Id);

        // This is Timesheet.Reject()'s own state-machine rule (Update 4) -
        // the handler doesn't add any logic on top of it, so this test is
        // really confirming the handler doesn't accidentally swallow it.
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        _timesheetRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}