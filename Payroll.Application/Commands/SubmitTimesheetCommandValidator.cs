using System;
using FluentValidation;

namespace Payroll.Application.Commands;

public class SubmitTimesheetCommandValidator : AbstractValidator<SubmitTimesheetCommand>
{
    public SubmitTimesheetCommandValidator()
    {
        RuleFor(c => c.EmployeeId).NotEmpty().WithMessage("EmployeeId is required.");

        RuleFor(c => c.WorkDate)
            .LessThanOrEqualTo(_ => DateTime.UtcNow)
            .WithMessage("WorkDate cannot be in the future.");

        RuleFor(c => c.HoursWorked)
            .GreaterThan(0m).WithMessage("HoursWorked must be greater than zero.")
            .LessThanOrEqualTo(24m).WithMessage("HoursWorked cannot exceed 24 in a single day.");
    }
}