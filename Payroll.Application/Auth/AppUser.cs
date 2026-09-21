namespace Payroll.Application.Auth;

public record AppUser(string Username, string PasswordHash, string Role);

public static class Roles
{
    public const string Employee = "Employee";
    public const string Manager = "Manager";
}