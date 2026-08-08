using System;
using Payroll.Domain.Entities;
using Payroll.Domain.Services;
using Xunit;

namespace Payroll.Domain.Tests;

public class PayrollCalculationServiceTests
{
    private readonly PayrollCalculationService _service = new();
    private readonly Employee _employee = new("Jane", "Doe", 20m);

    [Fact]
    public void Calculate_HoursAtOrBelowEight_IsAllRegularPayNoOvertime()
    {
        var timesheet = new Timesheet(_employee.Id, DateTime.UtcNow.AddDays(-1), 8m);
        var result = _service.Calculate(_employee, timesheet);

        Assert.Equal(8m, result.RegularHours);
        Assert.Equal(0m, result.OvertimeHours);
        Assert.Equal(160m, result.RegularPay);
        Assert.Equal(0m, result.OvertimePay);
        Assert.Equal(160m, result.TotalPay);
    }

    [Fact]
    public void Calculate_HoursAboveEight_SplitsIntoRegularAndOvertime()
    {
        var timesheet = new Timesheet(_employee.Id, DateTime.UtcNow.AddDays(-1), 13m);
        var result = _service.Calculate(_employee, timesheet);

        Assert.Equal(8m, result.RegularHours);
        Assert.Equal(5m, result.OvertimeHours);
        Assert.Equal(160m, result.RegularPay);
        Assert.Equal(150m, result.OvertimePay);
        Assert.Equal(310m, result.TotalPay);
    }

    [Fact]
    public void Calculate_PartialDay_HasNoOvertime()
    {
        var timesheet = new Timesheet(_employee.Id, DateTime.UtcNow.AddDays(-1), 4.5m);
        var result = _service.Calculate(_employee, timesheet);

        Assert.Equal(4.5m, result.RegularHours);
        Assert.Equal(0m, result.OvertimeHours);
        Assert.Equal(90m, result.RegularPay);
        Assert.Equal(90m, result.TotalPay);
    }
}