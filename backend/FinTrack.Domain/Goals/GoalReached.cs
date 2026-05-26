using FinTrack.Domain.Common;

namespace FinTrack.Domain.Goals;

public sealed record GoalReached(
    Guid GoalId,
    Guid UserId,
    Money Target,
    Money CurrentAmount) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
