using System;
using Payroll.Application.Commands;
using Xunit;

namespace Payroll.Application.Tests;

public class CreateEmployeeCommandValidatorTests
{
    private readonly CreateEmployeeCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var command = new CreateEmployeeCommand("Jane", "Doe", 25m);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "Doe", 25)]
    [InlineData("Jane", "", 25)]
    [InlineData("Jane", "Doe", 0)]
    [InlineData("Jane", "Doe", -5)]
    public void Validate_InvalidCommand_HasErrors(string firstName, string lastName, decimal hourlyRate)
    {
        var command = new CreateEmployeeCommand(firstName, lastName, hourlyRate);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }
}

public class SubmitTimesheetCommandValidatorTests
{
    private readonly SubmitTimesheetCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var command = new SubmitTimesheetCommand(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), 8m);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyEmployeeId_HasError()
    {
        var command = new SubmitTimesheetCommand(Guid.Empty, DateTime.UtcNow.AddDays(-1), 8m);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SubmitTimesheetCommand.EmployeeId));
    }

    [Fact]
    public void Validate_FutureWorkDate_HasError()
    {
        var command = new SubmitTimesheetCommand(Guid.NewGuid(), DateTime.UtcNow.AddDays(1), 8m);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SubmitTimesheetCommand.WorkDate));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(25)]
    public void Validate_HoursOutOfRange_HasError(decimal hours)
    {
        var command = new SubmitTimesheetCommand(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), hours);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SubmitTimesheetCommand.HoursWorked));
    }
}