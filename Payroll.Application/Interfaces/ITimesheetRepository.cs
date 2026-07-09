using System;
using System.Threading.Tasks;
using Payroll.Domain.Entities;

namespace Payroll.Application.Interfaces;

public interface ITimesheetRepository
{
    // Task represents an asynchronous operation, critical for enterprise performance
    Task<Timesheet?> GetByIdAsync(Guid id);
    Task AddAsync(Timesheet timesheet);
    Task SaveChangesAsync();
}