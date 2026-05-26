using FinTrack.Domain.Accounts;
using FinTrack.Domain.Common;
using FluentAssertions;

namespace FinTrack.Tests.Domain.Accounts;

public class AccountTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Open_ContaCorrenteComSaldoNegativo_LancaDomainException()
    {
        var act = () => Account.Open(UserId, "Corrente", AccountType.Checking, Money.Of(-1m));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Open_CartaoSemLimite_LancaDomainException()
    {
        var act = () => Account.Open(UserId, "Cartão", AccountType.CreditCard, Money.Zero());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Credit_AumentaOSaldo()
    {
        var account = Account.Open(UserId, "Corrente", AccountType.Checking, Money.Of(100m));

        account.Credit(Money.Of(50m));

        account.Balance.Should().Be(Money.Of(150m));
    }

    [Fact]
    public void Debit_ComValorZeroOuNegativo_LancaDomainException()
    {
        var account = Account.Open(UserId, "Corrente", AccountType.Checking, Money.Of(100m));

        account.Invoking(a => a.Debit(Money.Zero())).Should().Throw<DomainException>();
        account.Invoking(a => a.Debit(Money.Of(-5m))).Should().Throw<DomainException>();
    }

    [Fact]
    public void Debit_ContaCorrenteComSaldoInsuficiente_LancaDomainException()
    {
        var account = Account.Open(UserId, "Corrente", AccountType.Checking, Money.Of(100m));

        var act = () => account.Debit(Money.Of(150m));

        act.Should().Throw<DomainException>().WithMessage("*Insufficient*");
    }

    [Fact]
    public void Debit_ContaCorrenteComSaldoSuficiente_ReduzOSaldo()
    {
        var account = Account.Open(UserId, "Corrente", AccountType.Checking, Money.Of(100m));

        account.Debit(Money.Of(40m));

        account.Balance.Should().Be(Money.Of(60m));
    }

    [Fact]
    public void Debit_CartaoDentroDoLimite_PermiteSaldoNegativo()
    {
        var card = Account.Open(UserId, "Cartão", AccountType.CreditCard, Money.Zero(), Money.Of(1000m));

        card.Debit(Money.Of(800m));

        card.Balance.Should().Be(Money.Of(-800m));
    }

    [Fact]
    public void Debit_CartaoAlemDoLimite_LancaDomainException()
    {
        var card = Account.Open(UserId, "Cartão", AccountType.CreditCard, Money.Zero(), Money.Of(1000m));
        card.Debit(Money.Of(800m));

        var act = () => card.Debit(Money.Of(300m)); // resultaria em -1100, abaixo do piso -1000

        act.Should().Throw<DomainException>().WithMessage("*credit limit*");
    }
}
