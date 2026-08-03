using System;
using Payroll.Domain.Entities;
using Xunit;
namespace Payroll.Domain.Tests;
public class EmployeeTests
{
    [Fact]
    public void Constructor_ValidData_CreatesActiveEmployee()
    {
        var employee = new Employee("Jane", "Doe", 25.50m);
        Assert.Equal("Jane", employee.FirstName);
        Assert.Equal("Doe", employee.LastName);
        Assert.Equal(25.50m, employee.HourlyRate);
        Assert.True(employee.IsActive);
    }
    [Fact]
    public void Constructor_EmptyName_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Employee("", "Doe", 25m));
        Assert.Contains("Employee must have a full name", exception.Message);
    }
    [Fact]
    public void Constructor_InvalidHourlyRate_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Employee("Jane", "Doe", 0m));
        Assert.Contains("Hourly rate must be greater than zero", exception.Message);
    }
    [Fact]
    public void GiveRaise_ValidRate_UpdatesHourlyRate()
    {
        var employee = new Employee("Jane", "Doe", 25m);
        employee.GiveRaise(30m);
        Assert.Equal(30m, employee.HourlyRate);
    }
    [Fact]
    public void Terminate_SetsEmployeeInactive()
    {
        var employee = new Employee("Jane", "Doe", 25m);
        employee.Terminate();
        Assert.False(employee.IsActive);
    }
}