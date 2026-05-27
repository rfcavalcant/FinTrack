using FinTrack.Application.Common;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Identity;

namespace FinTrack.Application.Identity.Register;

public sealed class RegisterUserCommandHandler
    : ICommandHandler<RegisterUserCommand, AuthenticationResult>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthenticationResult> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = Email.Create(command.Email);

        if (await _users.ExistsByEmailAsync(email, cancellationToken))
            throw new DuplicateEmailException(email.Value);

        var passwordHash = _passwordHasher.Hash(command.Password);
        var user = User.Register(command.Name, email, passwordHash);

        _users.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(user);
        return new AuthenticationResult(user.Id, user.Name, user.Email.Value, token);
    }
}
