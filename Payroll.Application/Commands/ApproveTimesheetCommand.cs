using System;
using MediatR;

namespace Payroll.Application.Commands;

// OverrideReason is only mandatory *in practice* when the timesheet has
// flagged anomalies - the handler enforces that, not this record, since a
// plain record has no way to express a conditional requirement.
public record ApproveTimesheetCommand(Guid TimesheetId, string? OverrideReason = null) : IRequest;