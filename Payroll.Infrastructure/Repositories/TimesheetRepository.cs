using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Infrastructure.Data;

namespace Payroll.Infrastructure.Repositories;

// We explicitly state that this class implements the ITimesheetRepository interface
public class TimesheetRepository : ITimesheetRepository
{
    private readonly AppDbContext _context;

    // Dependency Injection provides the DbContext
    public TimesheetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Timesheet?> GetByIdAsync(Guid id)
    {
        // Finds the timesheet by its primary key
        return await _context.Timesheets.FindAsync(id);
    }

    public async Task AddAsync(Timesheet timesheet)
    {
        // Adds the entity to EF Core's memory tracker
        await _context.Timesheets.AddAsync(timesheet);
    }

    public async Task SaveChangesAsync()
    {
        // Actually executes the SQL INSERT/UPDATE commands against the database
        await _context.SaveChangesAsync();
    }
}