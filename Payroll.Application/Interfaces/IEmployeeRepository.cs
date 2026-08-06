using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Payroll.Domain.Entities;
namespace Payroll.Application.Interfaces;
public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id);
    Task<List<Employee>> GetAllAsync();
    Task AddAsync(Employee employee);
    Task SaveChangesAsync();
}