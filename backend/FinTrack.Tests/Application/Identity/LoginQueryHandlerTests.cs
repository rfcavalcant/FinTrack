using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Identity.Login;
using FinTrack.Domain.Identity;
using FluentAssertions;
using NSubstitute;

namespace FinTrack.Tests.Application.Identity;

public class LoginQueryHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();

    private LoginQueryHandler CreateHandler() => new(_users, _passwordHasher, _jwtTokenGenerator);

    private static User ExistingUser()
        => User.Register("Rafael", Email.Create("rafael@fintrack.com"), "HASHED");

    [Fact]
    public async Task HandleAsync_ComCredenciaisValidas_RetornaToken()
    {
        var user = ExistingUser();
        _users.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("senhaForte123", "HASHED").Returns(true);
        _jwtTokenGenerator.GenerateToken(user).Returns("TOKEN");

        var result = await CreateHandler().HandleAsync(
            new LoginQuery("rafael@fintrack.com", "senhaForte123"), CancellationToken.None);

        result.Token.Should().Be("TOKEN");
    }

    [Fact]
    public async Task HandleAsync_ComEmailInexistente_LancaInvalidCredentials()
    {
        _users.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => CreateHandler().HandleAsync(
            new LoginQuery("nao@existe.com", "qualquer"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task HandleAsync_ComSenhaIncorreta_LancaInvalidCredentials()
    {
        var user = ExistingUser();
        _users.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var act = () => CreateHandler().HandleAsync(
            new LoginQuery("rafael@fintrack.com", "senhaErrada"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }
}
