using MediatR;

namespace FinTrack.Application.Identity.Register;

public sealed record RegisterUserCommand(string Name, string Email, string Password)
    : IRequest<AuthenticationResult>;
