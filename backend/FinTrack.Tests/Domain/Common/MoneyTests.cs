using FinTrack.Domain.Common;
using FluentAssertions;

namespace FinTrack.Tests.Domain.Common;

public class MoneyTests
{
    [Fact]
    public void Add_ComMesmaMoeda_SomaOsValores()
    {
        var result = Money.Of(10m).Add(Money.Of(5m));

        result.Should().Be(Money.Of(15m));
    }

    [Fact]
    public void Subtract_ComMesmaMoeda_SubtraiOsValores()
    {
        var result = Money.Of(10m).Subtract(Money.Of(4m));

        result.Should().Be(Money.Of(6m));
    }

    [Fact]
    public void Add_ComMoedasDiferentes_LancaDomainException()
    {
        var act = () => Money.Of(10m, "BRL").Add(Money.Of(5m, "USD"));

        act.Should().Throw<DomainException>().WithMessage("*currencies*");
    }

    [Fact]
    public void Subtract_ComMoedasDiferentes_LancaDomainException()
    {
        var act = () => Money.Of(10m, "BRL").Subtract(Money.Of(5m, "USD"));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void IsGreaterThan_ComMoedasDiferentes_LancaDomainException()
    {
        var act = () => Money.Of(10m, "BRL").IsGreaterThan(Money.Of(5m, "USD"));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Of_ArredondaParaDuasCasasDecimais()
    {
        Money.Of(10.555m).Amount.Should().Be(10.56m);
    }

    [Fact]
    public void Of_ComMoedaVazia_LancaDomainException()
    {
        var act = () => Money.Of(10m, "  ");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Dois_ValoresIguais_SaoConsideradosEquivalentes()
    {
        // Moeda é normalizada para maiúsculas, então "brl" e "BRL" são equivalentes.
        Money.Of(10m, "brl").Should().Be(Money.Of(10m, "BRL"));
        (Money.Of(10m) == Money.Of(10m)).Should().BeTrue();
    }

    [Fact]
    public void IsNegative_e_IsZero_RefletemOValor()
    {
        Money.Of(-1m).IsNegative.Should().BeTrue();
        Money.Zero().IsZero.Should().BeTrue();
        Money.Of(5m).IsNegative.Should().BeFalse();
    }
}
