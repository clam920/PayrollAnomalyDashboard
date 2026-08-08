namespace Payroll.Domain.Enums;

// Each case corresponds to one detection rule in TimesheetAnomalyDetector.
// Keeping this as its own enum (instead of a free-text string) means the API
// and any future dashboard can group/filter anomalies reliably.
public enum AnomalyType
{
    ExcessiveHours,
    DuplicateWorkDate,
    WeekendWork,
    HighPayAmount
}