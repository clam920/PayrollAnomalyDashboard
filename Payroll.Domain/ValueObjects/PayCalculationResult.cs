namespace Payroll.Domain.ValueObjects;

// A "value object": has no Id, no identity of its own, and is fully defined
// by its data - two PayCalculationResults with the same numbers are
// interchangeable. It's a computed result, not something stored in the DB.
public record PayCalculationResult(decimal RegularHours, decimal OvertimeHours, decimal RegularPay, decimal OvertimePay)
{
    public decimal TotalPay => RegularPay + OvertimePay;
}