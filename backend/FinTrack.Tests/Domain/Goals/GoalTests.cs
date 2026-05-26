using FinTrack.Domain.Common;
using FinTrack.Domain.Goals;
using FluentAssertions;

namespace FinTrack.Tests.Domain.Goals;

public class GoalTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Define_ComAlvoZeroOuNegativo_LancaDomainException()
    {
        var act = () => Goal.Define(UserId, "Viagem", Money.Zero());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Contribute_ComValorNegativoOuZero_LancaDomainException()
    {
        var goal = Goal.Define(UserId, "Viagem", Money.Of(1000m));

        goal.Invoking(g => g.Contribute(Money.Zero())).Should().Throw<DomainException>();
        goal.Invoking(g => g.Contribute(Money.Of(-10m))).Should().Throw<DomainException>();
    }

    [Fact]
    public void Contribute_AoAtingirOAlvo_LevantaGoalReached()
    {
        var goal = Goal.Define(UserId, "Viagem", Money.Of(1000m));

        goal.Contribute(Money.Of(1000m));

        goal.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<GoalReached>();
        goal.IsReached.Should().BeTrue();
    }

    [Fact]
    public void Contribute_AbaixoDoAlvo_NaoLevantaEvento()
    {
        var goal = Goal.Define(UserId, "Viagem", Money.Of(1000m));

        goal.Contribute(Money.Of(400m));

        goal.DomainEvents.Should().BeEmpty();
        goal.IsReached.Should().BeFalse();
    }

    [Fact]
    public void Contribute_AposJaAtingido_NaoLevantaGoalReachedNovamente()
    {
        var goal = Goal.Define(UserId, "Viagem", Money.Of(1000m));
        goal.Contribute(Money.Of(1000m)); // atinge o alvo → 1 evento
        goal.ClearDomainEvents();

        goal.Contribute(Money.Of(200m)); // já estava atingido → não deve repetir

        goal.DomainEvents.Should().BeEmpty();
    }
}
