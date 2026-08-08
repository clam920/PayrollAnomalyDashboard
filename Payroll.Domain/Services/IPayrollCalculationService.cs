using Payroll.Domain.Entities;
using Payroll.Domain.ValueObjects;

namespace Payroll.Domain.Services;

public interface IPayrollCalculationService
{
    PayCalculationResult Calculate(Employee employee, Timesheet timesheet);
}