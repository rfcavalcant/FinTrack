using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Transactions;
using FinTrack.Domain.Accounts;
using FinTrack.Domain.Common;
using FinTrack.Domain.Transactions;
using FluentAssertions;
using NSubstitute;

namespace FinTrack.Tests.Application.Ledger;

public class DeleteTransactionCommandHandlerTests
{
    private readonly ITransactionRepository _transactions = Substitute.For<ITransactionRepository>();
    private readonly IAccountRepository _accounts = Substitute.For<IAccountRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly Guid _userId = Guid.NewGuid();

    public DeleteTransactionCommandHandlerTests() => _currentUser.UserId.Returns(_userId);

    private DeleteTransactionCommandHandler CreateHandler()
        => new(_transactions, _accounts, _unitOfWork, _currentUser);

    [Fact]
    public async Task HandleAsync_ExcluiDespesa_EstornaCreditandoDeVolta()
    {
        // Saldo 700 = 1000 inicial - 300 de uma despesa que será excluída.
        var account = Account.Open(_userId, "Corrente", AccountType.Checking, Money.Of(700m));
        var transaction = Transaction.RegisterExpense(
            _userId, account.Id, Guid.NewGuid(), Money.Of(300m), new DateOnly(2026, 5, 25), null);
        _transactions.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(transaction);
        _accounts.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(account);

        await CreateHandler().HandleAsync(new DeleteTransactionCommand(transaction.Id), CancellationToken.None);

        account.Balance.Should().Be(Money.Of(1000m));
        _transactions.Received(1).Remove(transaction);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ExcluiReceita_EstornaDebitandoDeVolta()
    {
        // Saldo 1250 = 1000 inicial + 250 de uma receita que será excluída.
        var account = Account.Open(_userId, "Corrente", AccountType.Checking, Money.Of(1250m));
        var transaction = Transaction.RegisterIncome(
            _userId, account.Id, Guid.NewGuid(), Money.Of(250m), new DateOnly(2026, 5, 25), null);
        _transactions.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(transaction);
        _accounts.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(account);

        await CreateHandler().HandleAsync(new DeleteTransactionCommand(transaction.Id), CancellationToken.None);

        account.Balance.Should().Be(Money.Of(1000m));
    }
}
