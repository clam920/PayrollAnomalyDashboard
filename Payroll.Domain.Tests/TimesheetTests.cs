using System;
using Payroll.Domain.Entities;
using Xunit;

namespace Payroll.Domain.Tests;

public class TimesheetTests
{
    [Fact]
    public void Constructor_ValidData_CreatesDraftTimesheet()
    {
        // Arrange: Set up the valid test data
        var employeeId = Guid.NewGuid();
        var workDate = DateTime.UtcNow.AddDays(-1); // Yesterday
        var hoursWorked = 8.5m; // 'm' explicitly tells C# this is a highly precise decimal, not a standard float

        // Act: Execute the code being tested
        var timesheet = new Timesheet(employeeId, workDate, hoursWorked);

        // Assert: Verify the results
        Assert.Equal(employeeId, timesheet.EmployeeId);
        Assert.Equal(8.5m, timesheet.HoursWorked);
        Assert.Equal(TimesheetStatus.Draft, timesheet.Status);
    }

    [Fact]
    public void Constructor_FutureDate_ThrowsArgumentException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(2); // Tomorrow
        var hoursWorked = 8m;

        // Act & Assert: We expect this specific action to crash with a specific error
        var exception = Assert.Throws<ArgumentException>(() => 
            new Timesheet(employeeId, futureDate, hoursWorked));

        Assert.Contains("Cannot log hours for a future date", exception.Message);
    }

    [Fact]
    public void Submit_DraftTimesheet_ChangesStatusToSubmitted()
    {
        // Arrange
        var timesheet = new Timesheet(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), 8m);

        // Act
        timesheet.Submit();

        // Assert
        Assert.Equal(TimesheetStatus.Submitted, timesheet.Status);
    }
}