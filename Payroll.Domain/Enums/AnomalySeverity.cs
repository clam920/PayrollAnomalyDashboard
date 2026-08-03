namespace Payroll.Domain.Enums;

// Ordered from least to most urgent so callers can do things like
// "Severity >= AnomalySeverity.Warning" if they want to filter later.
public enum AnomalySeverity
{
    Info,
    Warning,
    Critical
}