using MediatR;

namespace Payroll.Application.Commands;

public record LoginCommand(string Username, string Password) : IRequest<LoginResult>;

public record LoginResult(string Token, string Username, string Role);