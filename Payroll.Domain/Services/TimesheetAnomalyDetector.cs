using System;
using System.Collections.Generic;
using System.Linq;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;

namespace Payroll.Domain.Services;

// Concrete implementation of IAnomalyDetectionService. Each rule is its own
// private method returning early into a shared list - this keeps the rules
// independent of each other, so adding rule #4 next sprint won't risk
// breaking rules #1-3.
public class TimesheetAnomalyDetector : IAnomalyDetectionService
{
    // The Timesheet entity itself only rejects hours outside 0-24 (a hard
    // impossibility). These thresholds are softer, business-judgement lines:
    // "still physically possible, but worth a human looking at it".
    private const decimal WarningHoursThreshold = 12m;
    private const decimal CriticalHoursThreshold = 16m;

    // A single day's pay above this is worth a second look, regardless of
    // whether it came from long hours or a high hourly rate.
    private const decimal HighPayThreshold = 500m;

    private readonly IPayrollCalculationService _payrollCalculationService;

    // Depending on IPayrollCalculationService (not a concrete class) keeps
    // this testable with a fake/mock calculator if the pay rules ever need
    // to be swapped independently of the anomaly rules.
    public TimesheetAnomalyDetector(IPayrollCalculationService payrollCalculationService)
    {
        _payrollCalculationService = payrollCalculationService;
    }

    public IReadOnlyList<Anomaly> Detect(Employee employee, Timesheet timesheet, IEnumerable<Timesheet> recentTimesheets)
    {
        var anomalies = new List<Anomaly>();

        CheckExcessiveHours(timesheet, anomalies);
        CheckDuplicateWorkDate(timesheet, recentTimesheets, anomalies);
        CheckWeekendWork(timesheet, anomalies);
        CheckHighPayAmount(employee, timesheet, anomalies);

        return anomalies;
    }

    private static void CheckExcessiveHours(Timesheet timesheet, List<Anomaly> anomalies)
    {
        if (timesheet.HoursWorked >= CriticalHoursThreshold)
        {
            anomalies.Add(new Anomaly(
                timesheet.Id,
                timesheet.EmployeeId,
                AnomalyType.ExcessiveHours,
                AnomalySeverity.Critical,
                $"{timesheet.HoursWorked} hours logged on {timesheet.WorkDate:yyyy-MM-dd} is far above a normal shift and needs review."));
        }
        else if (timesheet.HoursWorked >= WarningHoursThreshold)
        {
            anomalies.Add(new Anomaly(
                timesheet.Id,
                timesheet.EmployeeId,
                AnomalyType.ExcessiveHours,
                AnomalySeverity.Warning,
                $"{timesheet.HoursWorked} hours logged on {timesheet.WorkDate:yyyy-MM-dd} exceeds the {WarningHoursThreshold}-hour threshold."));
        }
    }

    private static void CheckDuplicateWorkDate(Timesheet timesheet, IEnumerable<Timesheet> recentTimesheets, List<Anomaly> anomalies)
    {
        bool hasDuplicate = recentTimesheets.Any(existing =>
            existing.Id != timesheet.Id &&
            existing.EmployeeId == timesheet.EmployeeId &&
            existing.WorkDate.Date == timesheet.WorkDate.Date);

        if (hasDuplicate)
        {
            anomalies.Add(new Anomaly(
                timesheet.Id,
                timesheet.EmployeeId,
                AnomalyType.DuplicateWorkDate,
                AnomalySeverity.Warning,
                $"Another timesheet already exists for {timesheet.WorkDate:yyyy-MM-dd}. Possible duplicate entry."));
        }
    }

    private static void CheckWeekendWork(Timesheet timesheet, List<Anomaly> anomalies)
    {
        bool isWeekend = timesheet.WorkDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

        if (isWeekend)
        {
            // Info severity: this is often perfectly legitimate (retail, on-call,
            // overtime shifts), so it's surfaced for awareness rather than as a
            // problem to fix.
            anomalies.Add(new Anomaly(
                timesheet.Id,
                timesheet.EmployeeId,
                AnomalyType.WeekendWork,
                AnomalySeverity.Info,
                $"Hours logged on {timesheet.WorkDate:yyyy-MM-dd} fall on a {timesheet.WorkDate.DayOfWeek}."));
        }
    }

    private void CheckHighPayAmount(Employee employee, Timesheet timesheet, List<Anomaly> anomalies)
    {
        var pay = _payrollCalculationService.Calculate(employee, timesheet);

        if (pay.TotalPay >= HighPayThreshold)
        {
            anomalies.Add(new Anomaly(
                timesheet.Id,
                timesheet.EmployeeId,
                AnomalyType.HighPayAmount,
                AnomalySeverity.Warning,
                $"Calculated pay of {pay.TotalPay:C} for {timesheet.WorkDate:yyyy-MM-dd} exceeds the {HighPayThreshold:C} threshold for a single day."));
        }
    }
}