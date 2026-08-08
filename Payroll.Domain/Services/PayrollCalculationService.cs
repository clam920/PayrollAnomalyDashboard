using System;
using Payroll.Domain.Entities;
using Payroll.Domain.ValueObjects;

namespace Payroll.Domain.Services;

public class PayrollCalculationService : IPayrollCalculationService
{
    private const decimal RegularHoursThreshold = 8m;
    private const decimal OvertimeMultiplier = 1.5m;

    public PayCalculationResult Calculate(Employee employee, Timesheet timesheet)
    {
        var regularHours = Math.Min(timesheet.HoursWorked, RegularHoursThreshold);
        var overtimeHours = Math.Max(timesheet.HoursWorked - RegularHoursThreshold, 0m);

        var regularPay = regularHours * employee.HourlyRate;
        var overtimePay = overtimeHours * employee.HourlyRate * OvertimeMultiplier;

        return new PayCalculationResult(regularHours, overtimeHours, regularPay, overtimePay);
    }
}