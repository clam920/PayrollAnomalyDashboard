using System;

namespace Payroll.Domain.Entities;

public class Employee
{
    // Properties with private setters (Immutability)
    public Guid Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public decimal HourlyRate { get; private set; }
    public bool IsActive { get; private set; }

    // Constructor: Enforces the rules of creating a valid Employee
    public Employee(string firstName, string lastName, decimal hourlyRate)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Employee must have a full name.");
            
        if (hourlyRate <= 0)
            throw new ArgumentException("Hourly rate must be greater than zero.");

        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        HourlyRate = hourlyRate;
        IsActive = true; // Employees are active by default when created
    }

    // Behaviors (Domain Logic)
    public void GiveRaise(decimal newRate)
    {
        if (newRate <= HourlyRate)
            throw new ArgumentException("New rate must be strictly greater than the current rate.");

        HourlyRate = newRate;
    }

    public void Terminate()
    {
        IsActive = false;
    }
}