using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Payroll.Application.Auth;

namespace Payroll.Infrastructure.Auth;

public class InMemoryUserStore : IUserStore
{
    private readonly List<AppUser> _users;

    public InMemoryUserStore(IPasswordHasher<AppUser> passwordHasher)
    {
        _users = new List<AppUser>
        {
            CreateUser(passwordHasher, "manager", "Manager123!", Roles.Manager),
            CreateUser(passwordHasher, "employee", "Employee123!", Roles.Employee),
        };
    }

    public AppUser? FindByUsername(string username)
    {
        return _users.FirstOrDefault(u => u.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase));
    }

    private static AppUser CreateUser(IPasswordHasher<AppUser> passwordHasher, string username, string plainTextPassword, string role)
    {
        var placeholder = new AppUser(username, string.Empty, role);
        var hash = passwordHasher.HashPassword(placeholder, plainTextPassword);
        return placeholder with { PasswordHash = hash };
    }
}