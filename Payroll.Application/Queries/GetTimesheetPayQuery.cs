using System;
using MediatR;

namespace Payroll.Application.Queries;

public record GetTimesheetPayQuery(Guid TimesheetId) : IRequest<PayCalculationDto>;

public record PayCalculationDto(
    Guid TimesheetId,
    decimal RegularHours,
    decimal OvertimeHours,
    decimal RegularPay,
    decimal OvertimePay,
    decimal TotalPay);