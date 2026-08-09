using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Payroll.Application.Interfaces;

namespace Payroll.Application.Commands;

public class ApproveTimesheetCommandHandler : IRequestHandler<ApproveTimesheetCommand>
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IAnomalyRepository _anomalyRepository;

    public ApproveTimesheetCommandHandler(ITimesheetRepository timesheetRepository, IAnomalyRepository anomalyRepository)
    {
        _timesheetRepository = timesheetRepository;
        _anomalyRepository = anomalyRepository;
    }

    // MediatR 14's non-generic IRequestHandler<TRequest> expects a plain
    // Task, not Task<Unit> (that was the older MediatR convention) - hence
    // no "return Unit.Value" at the end.
    public async Task Handle(ApproveTimesheetCommand request, CancellationToken cancellationToken)
    {
        var timesheet = await _timesheetRepository.GetByIdAsync(request.TimesheetId);
        if (timesheet is null)
            throw new NotFoundException("Timesheet not found.");

        var anomalies = await _anomalyRepository.GetByTimesheetIdAsync(request.TimesheetId);
        if (anomalies.Count > 0 && string.IsNullOrWhiteSpace(request.OverrideReason))
        {
            throw new InvalidOperationException(
                $"This timesheet has {anomalies.Count} flagged anomaly(ies) and requires an override reason to approve.");
        }

        timesheet.Approve(request.OverrideReason);
        await _timesheetRepository.SaveChangesAsync();
    }
}