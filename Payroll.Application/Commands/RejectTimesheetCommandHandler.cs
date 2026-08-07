using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Payroll.Application.Interfaces;

namespace Payroll.Application.Commands;

public class RejectTimesheetCommandHandler : IRequestHandler<RejectTimesheetCommand>
{
    private readonly ITimesheetRepository _timesheetRepository;

    public RejectTimesheetCommandHandler(ITimesheetRepository timesheetRepository)
    {
        _timesheetRepository = timesheetRepository;
    }

    public async Task Handle(RejectTimesheetCommand request, CancellationToken cancellationToken)
    {
        var timesheet = await _timesheetRepository.GetByIdAsync(request.TimesheetId);
        if (timesheet is null)
            throw new ArgumentException("Timesheet not found.");

        timesheet.Reject(request.Reason);
        await _timesheetRepository.SaveChangesAsync();
    }
}