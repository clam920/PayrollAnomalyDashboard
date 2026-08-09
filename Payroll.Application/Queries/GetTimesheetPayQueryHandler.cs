using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Payroll.Application.Interfaces;
using Payroll.Domain.Services;

namespace Payroll.Application.Queries;

public class GetTimesheetPayQueryHandler : IRequestHandler<GetTimesheetPayQuery, PayCalculationDto>
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPayrollCalculationService _payrollCalculationService;

    public GetTimesheetPayQueryHandler(
        ITimesheetRepository timesheetRepository,
        IEmployeeRepository employeeRepository,
        IPayrollCalculationService payrollCalculationService)
    {
        _timesheetRepository = timesheetRepository;
        _employeeRepository = employeeRepository;
        _payrollCalculationService = payrollCalculationService;
    }

    public async Task<PayCalculationDto> Handle(GetTimesheetPayQuery request, CancellationToken cancellationToken)
    {
        var timesheet = await _timesheetRepository.GetByIdAsync(request.TimesheetId);
        if (timesheet is null)
            throw new NotFoundException("Timesheet not found.");

        var employee = await _employeeRepository.GetByIdAsync(timesheet.EmployeeId);
        if (employee is null)
            throw new NotFoundException("Employee for this timesheet no longer exists.");

        var pay = _payrollCalculationService.Calculate(employee, timesheet);

        return new PayCalculationDto(
            timesheet.Id, pay.RegularHours, pay.OvertimeHours, pay.RegularPay, pay.OvertimePay, pay.TotalPay);
    }
}