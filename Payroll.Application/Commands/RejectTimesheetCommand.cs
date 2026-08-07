using System;
using MediatR;

namespace Payroll.Application.Commands;

// Reason is optional here (unlike ApproveTimesheetCommand's OverrideReason):
// rejecting a flagged timesheet doesn't need special justification.
public record RejectTimesheetCommand(Guid TimesheetId, string? Reason = null) : IRequest;