using System;

namespace Payroll.Domain.Entities;

// 1. Defining States with an Enum
public enum TimesheetStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected
}

public class Timesheet
{
    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; } // The link back to the Employee
    public DateTime WorkDate { get; private set; }
    public decimal HoursWorked { get; private set; }
    public TimesheetStatus Status { get; private set; }

    // Constructor enforces creation rules
    public Timesheet(Guid employeeId, DateTime workDate, decimal hoursWorked)
    {
        if (hoursWorked <= 0 || hoursWorked > 24)
            throw new ArgumentException("Hours worked must be between 0.1 and 24.");

        if (workDate > DateTime.UtcNow)
            throw new ArgumentException("Cannot log hours for a future date.");

        Id = Guid.NewGuid();
        EmployeeId = employeeId;
        WorkDate = workDate;
        HoursWorked = hoursWorked;
        Status = TimesheetStatus.Draft; // Always starts as a draft
    }

    // Domain Logic: State Machine Transitions
    public void Submit()
    {
        if (Status != TimesheetStatus.Draft)
            throw new InvalidOperationException("Only draft timesheets can be submitted.");
            
        Status = TimesheetStatus.Submitted;
    }

    public void Approve()
    {
        if (Status != TimesheetStatus.Submitted)
            throw new InvalidOperationException("Timesheet must be submitted before approval.");
            
        Status = TimesheetStatus.Approved;
    }

    public void Reject()
    {
        if (Status == TimesheetStatus.Approved)
            throw new InvalidOperationException("Cannot reject an already approved timesheet. A correcting ledger entry is required.");
            
        Status = TimesheetStatus.Rejected;
    }
}