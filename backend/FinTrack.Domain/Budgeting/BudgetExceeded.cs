using FinTrack.Domain.Common;

namespace FinTrack.Domain.Budgeting;

public sealed record BudgetExceeded(
    Guid BudgetId,
    Guid UserId,
    Guid CategoryId,
    Money Limit,
    Money Consumption) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
