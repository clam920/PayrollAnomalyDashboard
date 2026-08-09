using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Payroll.Application.Exceptions;
using Payroll.Application.Interfaces;

namespace Payroll.Application.Commands;

public class ApproveTimesheetCommandHandler : IRequestHandler<ApproveTimesheetCommand>
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IAnomalyRepository _anomalyRepository;
    private readonly ILogger<ApproveTimesheetCommandHandler> _logger;

    public ApproveTimesheetCommandHandler(
        ITimesheetRepository timesheetRepository,
        IAnomalyRepository anomalyRepository,
        ILogger<ApproveTimesheetCommandHandler> logger)
    {
        _timesheetRepository = timesheetRepository;
        _anomalyRepository = anomalyRepository;
        _logger = logger;
    }

    // MediatR 14's non-generic IRequestHandler<TRequest> expects a plain
    // Task, not Task<Unit> (that was the older MediatR convention) - hence
    // no "return Unit.Value" at the end.
    public async Task Handle(ApproveTimesheetCommand request, CancellationToken cancellationToken)
    {
        var timesheet = await _timesheetRepository.GetByIdAsync(request.TimesheetId);
        if (timesheet is null)
            throw new NotFoundException("Timesheet not found.");

        // This is the key business rule for the whole feature: a flagged
        // timesheet can still be approved (the anomaly might be a false
        // positive, or a legitimate double shift) - but only with an
        // explicit, recorded reason. Silent approval of a flagged item
        // would defeat the point of flagging it in the first place.
        var anomalies = await _anomalyRepository.GetByTimesheetIdAsync(request.TimesheetId);
        if (anomalies.Count > 0 && string.IsNullOrWhiteSpace(request.OverrideReason))
        {
            throw new InvalidOperationException(
                $"This timesheet has {anomalies.Count} flagged anomaly(ies) and requires an override reason to approve.");
        }

        // Worth its own log line specifically when an override happened
        // (anomalies.Count > 0) - this is the audit trail for "a human
        // looked at a flagged item and consciously overrode it", which is
        // exactly the kind of event a payroll system should be able to
        // account for later.
        if (anomalies.Count > 0)
        {
            _logger.LogWarning(
                "Timesheet {TimesheetId} approved with {AnomalyCount} flagged anomaly(ies) overridden. Reason: {OverrideReason}",
                timesheet.Id, anomalies.Count, request.OverrideReason);
        }

        timesheet.Approve(request.OverrideReason);
        await _timesheetRepository.SaveChangesAsync();
    }
}