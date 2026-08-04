using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Payroll.Domain.Entities;

namespace Payroll.Application.Interfaces;

public interface ITimesheetRepository
{
    // Task represents an asynchronous operation, critical for enterprise performance
    Task<Timesheet?> GetByIdAsync(Guid id);
    Task AddAsync(Timesheet timesheet);
    Task SaveChangesAsync();

    // Feeds the AnomalyDetectionService's "recentTimesheets" parameter - e.g. so
    // it can spot a duplicate WorkDate. lookbackDays keeps the query bounded
    // instead of pulling an employee's entire multi-year history every time.
    Task<List<Timesheet>> GetRecentByEmployeeIdAsync(Guid employeeId, int lookbackDays = 30);
}