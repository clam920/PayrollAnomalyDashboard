using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Payroll.Domain.Entities;
using Payroll.Application.Interfaces;

namespace Payroll.Application.Commands;

public class SubmitTimesheetCommandHandler : IRequestHandler<SubmitTimesheetCommand, Guid>
{
    private readonly ITimesheetRepository _repository;

    // Dependency Injection: The .NET engine will automatically provide the database connection here
    public SubmitTimesheetCommandHandler(ITimesheetRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(SubmitTimesheetCommand request, CancellationToken cancellationToken)
    {
        // 1. Create the Domain Entity (The constructor validates the hours and date)
        var timesheet = new Timesheet(request.EmployeeId, request.WorkDate, request.HoursWorked);

        // 2. Execute the Domain Logic
        timesheet.Submit();

        // 3. Save to the database via our Interface abstraction
        await _repository.AddAsync(timesheet);
        await _repository.SaveChangesAsync();

        // 4. Return the ID so the web API can send it back to the client
        return timesheet.Id;
    }
}