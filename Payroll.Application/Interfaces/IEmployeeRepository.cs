using System;
using System.Threading.Tasks;
using Payroll.Domain.Entities;
namespace Payroll.Application.Interfaces;
public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id);
    Task AddAsync(Employee employee);
    Task SaveChangesAsync();
}