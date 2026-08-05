using System;
using MediatR;

namespace Payroll.Application.Commands;

// IRequest<SubmitTimesheetResult> tells MediatR: "When this command finishes,
// it will return the new timesheet's Id plus how many anomalies were flagged."
public record SubmitTimesheetCommand(Guid EmployeeId, DateTime WorkDate, decimal HoursWorked) : IRequest<SubmitTimesheetResult>;

// Kept as its own record (rather than just returning Guid) so the caller
// finds out immediately whether the submission needs review, without a
// separate round-trip to a GET endpoint.
public record SubmitTimesheetResult(Guid TimesheetId, int AnomalyCount);