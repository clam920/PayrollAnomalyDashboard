using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Payroll.Domain.Entities;
using Payroll.Application.Interfaces;

namespace Payroll.Application.Commands;

public class SubmitTimesheetCommandHandler : IRequestHandler<SubmitTimesheetCommand, Guid>
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IEmployeeRepository _employeeRepository;

    // Dependency Injection: The .NET engine will automatically provide the database connection here
    public SubmitTimesheetCommandHandler(
    ITimesheetRepository timesheetRepository,
    IEmployeeRepository employeeRepository)
    {
        _timesheetRepository = timesheetRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<Guid> Handle(SubmitTimesheetCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
        if (employee is null)
            throw new ArgumentException("Employee not found.");
        if (!employee.IsActive)
            throw new ArgumentException("Cannot submit timesheet for an inactive employee.");
        
        // 1. Create the Domain Entity (The constructor validates the hours and date)
        var timesheet = new Timesheet(request.EmployeeId, request.WorkDate, request.HoursWorked);

        // 2. Execute the Domain Logic
        timesheet.Submit();

        // 3. Save to the database via our Interface abstraction
        await _timesheetRepository.AddAsync(timesheet);
        await _timesheetRepository.SaveChangesAsync();


        // 4. Return the ID so the web API can send it back to the client
        return timesheet.Id;
    }
}