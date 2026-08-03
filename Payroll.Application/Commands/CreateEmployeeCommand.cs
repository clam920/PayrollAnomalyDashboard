using System;
using MediatR;
namespace Payroll.Application.Commands;
public record CreateEmployeeCommand(string FirstName, string LastName, decimal HourlyRate) : IRequest<Guid>;