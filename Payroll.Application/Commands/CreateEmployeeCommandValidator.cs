using FluentValidation;

namespace Payroll.Application.Commands;

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty().WithMessage("First name is required.");
        RuleFor(c => c.LastName).NotEmpty().WithMessage("Last name is required.");
        RuleFor(c => c.HourlyRate).GreaterThan(0m).WithMessage("Hourly rate must be greater than zero.");
    }
}