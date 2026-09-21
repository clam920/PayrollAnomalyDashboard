namespace Payroll.Application.Auth;

public interface IJwtTokenGenerator
{
    string GenerateToken(AppUser user);
}