using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Infrastructure.Data;

namespace Payroll.Infrastructure.Repositories;

public class TimesheetRepository : ITimesheetRepository
{
    private readonly AppDbContext _context;

    public TimesheetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Timesheet?> GetByIdAsync(Guid id)
    {
        return await _context.Timesheets.FindAsync(id);
    }

    public async Task AddAsync(Timesheet timesheet)
    {
        await _context.Timesheets.AddAsync(timesheet);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<Timesheet>> GetRecentByEmployeeIdAsync(Guid employeeId, int lookbackDays = 30)
    {
        var cutoff = DateTime.UtcNow.Date.AddDays(-lookbackDays);

        return await _context.Timesheets
            .Where(t => t.EmployeeId == employeeId && t.WorkDate >= cutoff)
            .ToListAsync();
    }

    public async Task<List<Timesheet>> GetAllAsync(Guid? employeeId = null)
    {
        var query = _context.Timesheets.AsQueryable();

        if (employeeId is not null)
            query = query.Where(t => t.EmployeeId == employeeId);

        return await query.ToListAsync();
    }
}