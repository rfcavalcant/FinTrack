using FinTrack.Application.Common;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Identity;

namespace FinTrack.Application.Identity.Login;

public sealed class LoginQueryHandler : IQueryHandler<LoginQuery, AuthenticationResult>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginQueryHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthenticationResult> HandleAsync(
        LoginQuery query,
        CancellationToken cancellationToken = default)
    {
        var email = Email.Create(query.Email);
        var user = await _users.GetByEmailAsync(email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(query.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        var token = _jwtTokenGenerator.GenerateToken(user);
        return new AuthenticationResult(user.Id, user.Name, user.Email.Value, token);
    }
}
