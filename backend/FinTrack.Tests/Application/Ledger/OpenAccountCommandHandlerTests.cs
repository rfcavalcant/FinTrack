using FinTrack.Application.Accounts;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Accounts;
using FluentAssertions;
using NSubstitute;

namespace FinTrack.Tests.Application.Ledger;

public class OpenAccountCommandHandlerTests
{
    private readonly IAccountRepository _accounts = Substitute.For<IAccountRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly Guid _userId = Guid.NewGuid();

    public OpenAccountCommandHandlerTests() => _currentUser.UserId.Returns(_userId);

    private OpenAccountCommandHandler CreateHandler() => new(_accounts, _unitOfWork, _currentUser);

    [Fact]
    public async Task Handle_AbreContaParaOUsuarioAtual_Persiste()
    {
        var command = new OpenAccountCommand("Corrente", AccountType.Checking, 500m, "BRL", null);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.Balance.Should().Be(500m);
        result.Currency.Should().Be("BRL");
        result.Type.Should().Be("Checking");
        _accounts.Received(1).Add(Arg.Is<Account>(a => a.UserId == _userId));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
