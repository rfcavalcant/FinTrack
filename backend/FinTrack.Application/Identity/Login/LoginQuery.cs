using MediatR;

namespace FinTrack.Application.Identity.Login;

public sealed record LoginQuery(string Email, string Password)
    : IRequest<AuthenticationResult>;
