using FinTrack.Application.Common;
using FinTrack.Domain.Common;
using FinTrack.Domain.Transactions;
using FinTrack.Infrastructure.Persistence;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FinTrack.Tests.Infrastructure;

public class UnitOfWorkTests
{
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();

    private FinTrackDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task SaveChangesAsync_WithDomainEvents_DispatchesNotificationForEachEvent()
    {
        await using var ctx = CreateContext();
        var sut = new UnitOfWork(ctx, _publisher);

        var transaction = Transaction.RegisterIncome(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Money.Of(250m), DateOnly.FromDateTime(DateTime.Today));
        ctx.Transactions.Add(transaction);

        await sut.SaveChangesAsync();

        await _publisher.Received(1)
            .Publish(
                Arg.Is<object>(n => n is DomainEventNotification<TransactionRegistered>),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveChangesAsync_ClearsDomainEventsFromAggregateAfterSuccessfulCommit()
    {
        await using var ctx = CreateContext();
        var sut = new UnitOfWork(ctx, _publisher);

        var transaction = Transaction.RegisterIncome(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Money.Of(100m), DateOnly.FromDateTime(DateTime.Today));
        ctx.Transactions.Add(transaction);

        transaction.DomainEvents.Should().HaveCount(1);

        await sut.SaveChangesAsync();

        transaction.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_WithNoTrackedAggregates_DoesNotCallPublish()
    {
        await using var ctx = CreateContext();
        var sut = new UnitOfWork(ctx, _publisher);

        await sut.SaveChangesAsync();

        await _publisher.DidNotReceive()
            .Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveChangesAsync_WhenCalledTwice_DispatchesEventsOnlyOnce()
    {
        // Garante que o clear pós-commit previne double-dispatch numa segunda chamada.
        await using var ctx = CreateContext();
        var sut = new UnitOfWork(ctx, _publisher);

        var transaction = Transaction.RegisterIncome(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Money.Of(100m), DateOnly.FromDateTime(DateTime.Today));
        ctx.Transactions.Add(transaction);

        await sut.SaveChangesAsync();
        await sut.SaveChangesAsync(); // segunda chamada — agregado sem eventos

        await _publisher.Received(1)
            .Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveChangesAsync_DispatchesEventWithCorrectPayload()
    {
        await using var ctx = CreateContext();
        var sut = new UnitOfWork(ctx, _publisher);

        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var amount = Money.Of(500m);
        var date = DateOnly.FromDateTime(DateTime.Today);

        var transaction = Transaction.RegisterExpense(userId, accountId, categoryId, amount, date);
        ctx.Transactions.Add(transaction);

        await sut.SaveChangesAsync();

        await _publisher.Received(1)
            .Publish(
                Arg.Is<object>(n =>
                    n is DomainEventNotification<TransactionRegistered> &&
                    ((DomainEventNotification<TransactionRegistered>)n).Event.TransactionId == transaction.Id &&
                    ((DomainEventNotification<TransactionRegistered>)n).Event.UserId == userId &&
                    ((DomainEventNotification<TransactionRegistered>)n).Event.AccountId == accountId &&
                    ((DomainEventNotification<TransactionRegistered>)n).Event.Type == TransactionType.Expense),
                Arg.Any<CancellationToken>());
    }
}
