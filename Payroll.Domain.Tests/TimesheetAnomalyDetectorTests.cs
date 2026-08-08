using System;
using System.Linq;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;
using Payroll.Domain.Services;
using Xunit;

namespace Payroll.Domain.Tests;

public class TimesheetAnomalyDetectorTests
{
    // Existing tests below only assert Contains(...) for the anomaly type
    // they care about, so a timesheet also tripping HighPayAmount (e.g. the
    // 18-hour Critical case, at $25/hr) doesn't break them - they just don't
    // check for its absence.
    private readonly TimesheetAnomalyDetector _detector = new(new PayrollCalculationService());
    private readonly Employee _employee = new("Jane", "Doe", 25m);

    [Fact]
    public void Detect_NormalWeekdayShortShift_ReturnsNoHoursOrDuplicateAnomalies()
    {
        var weekday = GetLastWeekday();
        var timesheet = new Timesheet(_employee.Id, weekday, 8m);

        var result = _detector.Detect(_employee, timesheet, Array.Empty<Timesheet>());

        Assert.DoesNotContain(result, a => a.Type == AnomalyType.ExcessiveHours);
        Assert.DoesNotContain(result, a => a.Type == AnomalyType.DuplicateWorkDate);
        Assert.DoesNotContain(result, a => a.Type == AnomalyType.WeekendWork);
    }

    [Fact]
    public void Detect_HoursAtWarningThreshold_FlagsExcessiveHoursWarning()
    {
        var timesheet = new Timesheet(_employee.Id, GetLastWeekday(), 13m);

        var result = _detector.Detect(_employee, timesheet, Array.Empty<Timesheet>());

        Assert.Contains(result, a => a.Type == AnomalyType.ExcessiveHours && a.Severity == AnomalySeverity.Warning);
    }

    [Fact]
    public void Detect_HoursAtCriticalThreshold_FlagsExcessiveHoursCritical()
    {
        var timesheet = new Timesheet(_employee.Id, GetLastWeekday(), 18m);

        var result = _detector.Detect(_employee, timesheet, Array.Empty<Timesheet>());

        Assert.Contains(result, a => a.Type == AnomalyType.ExcessiveHours && a.Severity == AnomalySeverity.Critical);
    }

    [Fact]
    public void Detect_SameDateAsExistingTimesheet_FlagsDuplicateWorkDate()
    {
        var workDate = GetLastWeekday();
        var existing = new Timesheet(_employee.Id, workDate, 6m);
        var newTimesheet = new Timesheet(_employee.Id, workDate, 4m);

        var result = _detector.Detect(_employee, newTimesheet, new[] { existing });

        Assert.Contains(result, a => a.Type == AnomalyType.DuplicateWorkDate);
    }

    [Fact]
    public void Detect_DifferentEmployeeSameDate_DoesNotFlagDuplicateWorkDate()
    {
        var otherEmployee = new Employee("John", "Smith", 20m);
        var workDate = GetLastWeekday();
        var existing = new Timesheet(otherEmployee.Id, workDate, 6m);
        var newTimesheet = new Timesheet(_employee.Id, workDate, 4m);

        var result = _detector.Detect(_employee, newTimesheet, new[] { existing });

        Assert.DoesNotContain(result, a => a.Type == AnomalyType.DuplicateWorkDate);
    }

    [Fact]
    public void Detect_PayAtOrAboveThreshold_FlagsHighPayAmount()
    {
        // $50/hr * 13 hours = 8*50 + 5*50*1.5 = 400 + 375 = 775, over the $500 threshold.
        var highRateEmployee = new Employee("Alex", "Ng", 50m);
        var timesheet = new Timesheet(highRateEmployee.Id, GetLastWeekday(), 13m);

        var result = _detector.Detect(highRateEmployee, timesheet, Array.Empty<Timesheet>());

        Assert.Contains(result, a => a.Type == AnomalyType.HighPayAmount && a.Severity == AnomalySeverity.Warning);
    }

    [Fact]
    public void Detect_PayBelowThreshold_DoesNotFlagHighPayAmount()
    {
        // $25/hr * 8 hours = 200, well under the $500 threshold.
        var timesheet = new Timesheet(_employee.Id, GetLastWeekday(), 8m);

        var result = _detector.Detect(_employee, timesheet, Array.Empty<Timesheet>());

        Assert.DoesNotContain(result, a => a.Type == AnomalyType.HighPayAmount);
    }

    [Fact]
    public void Detect_WeekendWorkDate_FlagsInfoSeverity()
    {
        var saturday = GetLastDayOfWeek(DayOfWeek.Saturday);
        var timesheet = new Timesheet(_employee.Id, saturday, 6m);

        var result = _detector.Detect(_employee, timesheet, Array.Empty<Timesheet>());

        Assert.Contains(result, a => a.Type == AnomalyType.WeekendWork && a.Severity == AnomalySeverity.Info);
    }

    // Helpers: Timesheet's constructor rejects future dates, so tests need a
    // date that is (a) in the past and (b) a specific day-of-week.
    private static DateTime GetLastWeekday()
    {
        var date = DateTime.UtcNow.AddDays(-1);
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            date = date.AddDays(-1);
        return date;
    }

    private static DateTime GetLastDayOfWeek(DayOfWeek dayOfWeek)
    {
        var date = DateTime.UtcNow.AddDays(-1);
        while (date.DayOfWeek != dayOfWeek)
            date = date.AddDays(-1);
        return date;
    }
}