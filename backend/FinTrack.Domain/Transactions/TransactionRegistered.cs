using FinTrack.Domain.Common;

namespace FinTrack.Domain.Transactions;

public sealed record TransactionRegistered(
    Guid TransactionId,
    Guid UserId,
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    Money Amount,
    DateOnly Date) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
