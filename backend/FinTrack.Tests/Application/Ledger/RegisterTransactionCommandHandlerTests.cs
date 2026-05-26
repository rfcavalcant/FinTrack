using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Transactions;
using FinTrack.Domain.Accounts;
using FinTrack.Domain.Categories;
using FinTrack.Domain.Common;
using FinTrack.Domain.Transactions;
using FluentAssertions;
using NSubstitute;

namespace FinTrack.Tests.Application.Ledger;

public class RegisterTransactionCommandHandlerTests
{
    private readonly IAccountRepository _accounts = Substitute.For<IAccountRepository>();
    private readonly ICategoryRepository _categories = Substitute.For<ICategoryRepository>();
    private readonly ITransactionRepository _transactions = Substitute.For<ITransactionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly Guid _userId = Guid.NewGuid();

    public RegisterTransactionCommandHandlerTests() => _currentUser.UserId.Returns(_userId);

    private RegisterTransactionCommandHandler CreateHandler()
        => new(_accounts, _categories, _transactions, _unitOfWork, _currentUser);

    private Account ArrangeAccount(decimal initialBalance = 1000m)
    {
        var account = Account.Open(_userId, "Corrente", AccountType.Checking, Money.Of(initialBalance));
        _accounts.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(account);
        return account;
    }

    private void ArrangeCategory(CategoryType type)
    {
        var category = Category.Create(_userId, "Categoria", type, "#fff");
        _categories.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(category);
    }

    private static RegisterTransactionCommand Command(TransactionType type, decimal amount = 100m)
        => new(Guid.NewGuid(), Guid.NewGuid(), type, amount, new DateOnly(2026, 5, 25), "teste");

    [Fact]
    public async Task Handle_Despesa_DebitaOSaldoEPersiste()
    {
        var account = ArrangeAccount(1000m);
        ArrangeCategory(CategoryType.Expense);

        var result = await CreateHandler().Handle(Command(TransactionType.Expense, 300m), CancellationToken.None);

        account.Balance.Should().Be(Money.Of(700m));
        result.Type.Should().Be("Expense");
        _transactions.Received(1).Add(Arg.Any<Transaction>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Receita_CreditaOSaldo()
    {
        var account = ArrangeAccount(1000m);
        ArrangeCategory(CategoryType.Income);

        await CreateHandler().Handle(Command(TransactionType.Income, 250m), CancellationToken.None);

        account.Balance.Should().Be(Money.Of(1250m));
    }

    [Fact]
    public async Task Handle_ContaInexistente_LancaNotFound()
    {
        _accounts.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Account?)null);
        ArrangeCategory(CategoryType.Expense);

        var act = () => CreateHandler().Handle(Command(TransactionType.Expense), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CategoriaInexistente_LancaNotFound()
    {
        ArrangeAccount();
        _categories.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var act = () => CreateHandler().Handle(Command(TransactionType.Expense), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_TipoDeCategoriaIncompativelComOLancamento_LancaDomainException()
    {
        ArrangeAccount();
        ArrangeCategory(CategoryType.Income); // categoria de receita...

        var act = () => CreateHandler().Handle(Command(TransactionType.Expense), CancellationToken.None); // ...usada em despesa

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_DespesaMaiorQueOSaldo_LancaDomainException()
    {
        ArrangeAccount(100m);
        ArrangeCategory(CategoryType.Expense);

        var act = () => CreateHandler().Handle(Command(TransactionType.Expense, 500m), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}
