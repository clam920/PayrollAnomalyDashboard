using System;
using MediatR;

namespace Payroll.Application.Commands;

// IRequest<Guid> tells MediatR: "When this command finishes, it will return the Guid of the new timesheet."
public record SubmitTimesheetCommand(Guid EmployeeId, DateTime WorkDate, decimal HoursWorked) : IRequest<Guid>;