using FinTrack.Application.Budgeting;
using FinTrack.Application.Common;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Budgeting;
using FinTrack.Domain.Common;
using FinTrack.Domain.Transactions;
using FluentAssertions;
using NSubstitute;

namespace FinTrack.Tests.Application.Budgeting;

public class OnTransactionRegisteredHandlerTests
{
    private readonly IBudgetRepository _budgets = Substitute.For<IBudgetRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly OnTransactionRegisteredHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    public OnTransactionRegisteredHandlerTests()
        => _handler = new OnTransactionRegisteredHandler(_budgets, _unitOfWork);

    private DomainEventNotification<TransactionRegistered> MakeNotification(
        TransactionType type, decimal amount = 300m)
    {
        var evt = new TransactionRegistered(
            Guid.NewGuid(), UserId, Guid.NewGuid(), CategoryId,
            type, Money.Of(amount), Today);
        return new DomainEventNotification<TransactionRegistered>(evt);
    }

    [Fact]
    public async Task Handle_WithExpenseAndActiveBudget_RegistersConsumptionAndSaves()
    {
        var budget = Budget.Define(UserId, CategoryId,
            DateRange.ForMonth(Today.Year, Today.Month), Money.Of(1000m));
        _budgets.FindActiveAsync(UserId, CategoryId, Today, Arg.Any<CancellationToken>())
            .Returns(budget);

        await _handler.Handle(MakeNotification(TransactionType.Expense, 300m), CancellationToken.None);

        budget.Consumption.Amount.Should().Be(300m);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExpenseAndNoBudget_ReturnsSilentlyWithoutSaving()
    {
        _budgets.FindActiveAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((Budget?)null);

        await _handler.Handle(MakeNotification(TransactionType.Expense), CancellationToken.None);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithIncomeTransaction_IgnoresEventWithoutQueryingRepository()
    {
        await _handler.Handle(MakeNotification(TransactionType.Income), CancellationToken.None);

        await _budgets.DidNotReceive()
            .FindActiveAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExpenseThatExceedsBudget_BudgetExceededRaisedOnAggregate()
    {
        var budget = Budget.Define(UserId, CategoryId,
            DateRange.ForMonth(Today.Year, Today.Month), Money.Of(500m));
        _budgets.FindActiveAsync(UserId, CategoryId, Today, Arg.Any<CancellationToken>())
            .Returns(budget);

        await _handler.Handle(MakeNotification(TransactionType.Expense, 700m), CancellationToken.None);

        budget.IsExceeded.Should().BeTrue();
        budget.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<BudgetExceeded>();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExpenseAccumulatingAcrossMultipleTransactions_TracksTotalCorrectly()
    {
        var budget = Budget.Define(UserId, CategoryId,
            DateRange.ForMonth(Today.Year, Today.Month), Money.Of(1000m));
        _budgets.FindActiveAsync(UserId, CategoryId, Today, Arg.Any<CancellationToken>())
            .Returns(budget);

        // Primeiro lançamento
        await _handler.Handle(MakeNotification(TransactionType.Expense, 400m), CancellationToken.None);
        // Simula segundo lançamento (budget já carregado no teste)
        await _handler.Handle(MakeNotification(TransactionType.Expense, 400m), CancellationToken.None);

        budget.Consumption.Amount.Should().Be(800m);
        budget.IsExceeded.Should().BeFalse();
    }
}
