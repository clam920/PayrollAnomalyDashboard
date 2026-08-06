using System;
using System.Collections.Generic;
using MediatR;

namespace Payroll.Application.Queries;

public record GetEmployeesQuery : IRequest<List<EmployeeDto>>;

// A DTO rather than returning the Employee entity directly. This keeps the
// API contract stable even if the entity's internal shape changes later
// (e.g. we add an audit trail or navigation properties that shouldn't be
// serialized straight to JSON).
public record EmployeeDto(Guid Id, string FirstName, string LastName, decimal HourlyRate, bool IsActive);