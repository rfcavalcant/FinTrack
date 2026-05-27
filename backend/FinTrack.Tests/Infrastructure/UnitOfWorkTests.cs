using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Common;
using FinTrack.Domain.Transactions;
using FinTrack.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FinTrack.Tests.Infrastructure;

public class UnitOfWorkTests
{
    private readonly IDomainEventDispatcher _dispatcher = Substitute.For<IDomainEventDispatcher>();

    private FinTrackDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task SaveChangesAsync_WithDomainEvents_DispatchesEachEvent()
    {
        await using var ctx = CreateContext();
        var sut = new UnitOfWork(ctx, _dispatcher);

        var transaction = Transaction.RegisterIncome(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Money.Of(250m), DateOnly.FromDateTime(DateTime.Today));
        ctx.Transactions.Add(transaction);

        await sut.SaveChangesAsync();

        await _dispatcher.Received(1)
            .DispatchAsync(
                Arg.Is<IDomainEvent>(e => e is TransactionRegistered),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveChangesAsync_ClearsDomainEventsFromAggregateAfterSuccessfulCommit()
    {
        await using var ctx = CreateContext();
        var sut = new UnitOfWork(ctx, _dispatcher);

        var transaction = Transaction.RegisterIncome(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Money.Of(100m), DateOnly.FromDateTime(DateTime.Today));
        ctx.Transactions.Add(transaction);

        transaction.DomainEvents.Should().HaveCount(1);

        await sut.SaveChangesAsync();

        transaction.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_WithNoTrackedAggregates_DoesNotDispatch()
    {
        await using var ctx = CreateContext();
        var sut = new UnitOfWork(ctx, _dispatcher);

        await sut.SaveChangesAsync();

        await _dispatcher.DidNotReceive()
            .DispatchAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveChangesAsync_WhenCalledTwice_DispatchesEventsOnlyOnce()
    {
        // Garante que o clear pós-commit previne double-dispatch numa segunda chamada.
        await using var ctx = CreateContext();
        var sut = new UnitOfWork(ctx, _dispatcher);

        var transaction = Transaction.RegisterIncome(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Money.Of(100m), DateOnly.FromDateTime(DateTime.Today));
        ctx.Transactions.Add(transaction);

        await sut.SaveChangesAsync();
        await sut.SaveChangesAsync(); // segunda chamada — agregado sem eventos

        await _dispatcher.Received(1)
            .DispatchAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveChangesAsync_DispatchesEventWithCorrectPayload()
    {
        await using var ctx = CreateContext();
        var sut = new UnitOfWork(ctx, _dispatcher);

        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var amount = Money.Of(500m);
        var date = DateOnly.FromDateTime(DateTime.Today);

        var transaction = Transaction.RegisterExpense(userId, accountId, categoryId, amount, date);
        ctx.Transactions.Add(transaction);

        await sut.SaveChangesAsync();

        await _dispatcher.Received(1)
            .DispatchAsync(
                Arg.Is<IDomainEvent>(e =>
                    e is TransactionRegistered &&
                    ((TransactionRegistered)e).TransactionId == transaction.Id &&
                    ((TransactionRegistered)e).UserId == userId &&
                    ((TransactionRegistered)e).AccountId == accountId &&
                    ((TransactionRegistered)e).Type == TransactionType.Expense),
                Arg.Any<CancellationToken>());
    }
}
