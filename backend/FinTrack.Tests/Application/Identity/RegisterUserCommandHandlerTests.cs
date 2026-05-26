using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Identity.Register;
using FinTrack.Domain.Identity;
using FluentAssertions;
using NSubstitute;

namespace FinTrack.Tests.Application.Identity;

public class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private static readonly RegisterUserCommand Command =
        new("Rafael", "rafael@fintrack.com", "senhaForte123");

    private RegisterUserCommandHandler CreateHandler()
        => new(_users, _passwordHasher, _jwtTokenGenerator, _unitOfWork);

    [Fact]
    public async Task Handle_ComEmailNovo_FazHashCriaUsuarioEPersiste()
    {
        _users.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(false);
        _passwordHasher.Hash(Command.Password).Returns("HASHED");
        _jwtTokenGenerator.GenerateToken(Arg.Any<User>()).Returns("TOKEN");

        var result = await CreateHandler().Handle(Command, CancellationToken.None);

        result.Token.Should().Be("TOKEN");
        result.Email.Should().Be("rafael@fintrack.com");
        _passwordHasher.Received(1).Hash(Command.Password);
        _users.Received(1).Add(Arg.Is<User>(u => u.PasswordHash == "HASHED"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ComEmailJaCadastrado_LancaDuplicateEmailExceptionENaoPersiste()
    {
        _users.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(true);

        var act = () => CreateHandler().Handle(Command, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateEmailException>();
        _users.DidNotReceive().Add(Arg.Any<User>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
