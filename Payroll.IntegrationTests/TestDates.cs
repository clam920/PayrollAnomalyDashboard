using System;

namespace Payroll.IntegrationTests;

internal static class TestDates
{
    public static DateTime GetLastWeekday()
    {
        var date = DateTime.UtcNow.AddDays(-1);
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            date = date.AddDays(-1);
        return date;
    }
}