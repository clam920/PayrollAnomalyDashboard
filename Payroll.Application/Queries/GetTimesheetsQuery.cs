using System;
using System.Collections.Generic;
using MediatR;

namespace Payroll.Application.Queries;

// EmployeeId is optional: null means "everyone's timesheets".
public record GetTimesheetsQuery(Guid? EmployeeId = null) : IRequest<List<TimesheetDto>>;

// Status is exposed as a string (not the raw enum) so the JSON reads as
// "Submitted" instead of a bare integer - much easier to consume from a
// frontend or Swagger without cross-referencing the enum definition.
public record TimesheetDto(Guid Id, Guid EmployeeId, DateTime WorkDate, decimal HoursWorked, string Status);