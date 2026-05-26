using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Identity;
using MediatR;

namespace FinTrack.Application.Identity.Register;

public sealed class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, AuthenticationResult>
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

    public async Task<AuthenticationResult> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        if (await _users.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new DuplicateEmailException(email.Value);
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Register(request.Name, email, passwordHash);

        _users.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(user);
        return new AuthenticationResult(user.Id, user.Name, user.Email.Value, token);
    }
}
