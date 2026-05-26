using FinTrack.Domain.Budgeting;
using FinTrack.Domain.Common;
using FluentAssertions;

namespace FinTrack.Tests.Domain.Budgeting;

public class BudgetTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();

    private static Budget NewBudget(decimal limit = 1000m)
        => Budget.Define(UserId, CategoryId, DateRange.ForMonth(2026, 5), Money.Of(limit));

    [Fact]
    public void Define_ComLimiteZeroOuNegativo_LancaDomainException()
    {
        var act = () => Budget.Define(UserId, CategoryId, DateRange.ForMonth(2026, 5), Money.Zero());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RegisterConsumption_ComValorNegativo_LancaDomainException()
    {
        var budget = NewBudget();

        budget.Invoking(b => b.RegisterConsumption(Money.Of(-1m))).Should().Throw<DomainException>();
    }

    [Fact]
    public void RegisterConsumption_AbaixoDoLimite_NaoLevantaEvento()
    {
        var budget = NewBudget(1000m);

        budget.RegisterConsumption(Money.Of(600m));

        budget.DomainEvents.Should().BeEmpty();
        budget.IsExceeded.Should().BeFalse();
    }

    [Fact]
    public void RegisterConsumption_AcimaDoLimite_LevantaBudgetExceeded()
    {
        var budget = NewBudget(1000m);

        budget.RegisterConsumption(Money.Of(1200m));

        budget.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<BudgetExceeded>();
        budget.IsExceeded.Should().BeTrue();
    }

    [Fact]
    public void RegisterConsumption_Acumula_E_EstouraAoCruzarOLimite()
    {
        var budget = NewBudget(1000m);

        budget.RegisterConsumption(Money.Of(700m)); // abaixo do limite
        budget.DomainEvents.Should().BeEmpty();

        budget.RegisterConsumption(Money.Of(400m)); // acumula 1100 → cruza o limite
        budget.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<BudgetExceeded>();
    }
}
