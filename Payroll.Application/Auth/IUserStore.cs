namespace Payroll.Application.Auth;

public interface IUserStore
{
    AppUser? FindByUsername(string username);
}