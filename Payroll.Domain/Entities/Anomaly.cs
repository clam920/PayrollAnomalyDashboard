using System;
using Payroll.Domain.Enums;

namespace Payroll.Domain.Entities;

// Represents a single flagged issue on a Timesheet. This is intentionally a
// "dumb" record of what was found and why - the actual detection logic lives
// in TimesheetAnomalyDetector (a domain service), not here. Keeping the rules
// out of the entity means we can add/change rules without touching the shape
// of the data we persist.
public class Anomaly
{
    public Guid Id { get; private set; }
    public Guid TimesheetId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public AnomalyType Type { get; private set; }
    public AnomalySeverity Severity { get; private set; }
    public string Description { get; private set; }
    public DateTime DetectedAtUtc { get; private set; }

    public Anomaly(Guid timesheetId, Guid employeeId, AnomalyType type, AnomalySeverity severity, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Anomaly must have a description explaining why it was flagged.");

        Id = Guid.NewGuid();
        TimesheetId = timesheetId;
        EmployeeId = employeeId;
        Type = type;
        Severity = severity;
        Description = description;
        DetectedAtUtc = DateTime.UtcNow;
    }
}