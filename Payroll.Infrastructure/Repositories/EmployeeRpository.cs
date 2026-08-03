using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Infrastructure.Data;
namespace Payroll.Infrastructure.Repositories;
public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;
    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<Employee?> GetByIdAsync(Guid id)
    {
        return await _context.Employees.FindAsync(id);
    }
    public async Task AddAsync(Employee employee)
    {
        await _context.Employees.AddAsync(employee);
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}