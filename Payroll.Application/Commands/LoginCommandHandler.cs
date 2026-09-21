using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Payroll.Application.Auth;

namespace Payroll.Application.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IUserStore _userStore;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginCommandHandler(IUserStore userStore, IPasswordHasher<AppUser> passwordHasher, IJwtTokenGenerator tokenGenerator)
    {
        _userStore = userStore;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = _userStore.FindByUsername(request.Username);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid username or password.");

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid username or password.");

        var token = _tokenGenerator.GenerateToken(user);
        return Task.FromResult(new LoginResult(token, user.Username, user.Role));
    }
}