using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Payroll.Domain.Entities;

namespace Payroll.Application.Interfaces;

public interface ITimesheetRepository
{
    Task<Timesheet?> GetByIdAsync(Guid id);
    Task AddAsync(Timesheet timesheet);
    Task SaveChangesAsync();
    Task<List<Timesheet>> GetRecentByEmployeeIdAsync(Guid employeeId, int lookbackDays = 30);

    // For the GET /api/timesheets endpoint. employeeId is optional so the
    // same method serves both "list everything" and "list for one employee".
    Task<List<Timesheet>> GetAllAsync(Guid? employeeId = null);
}