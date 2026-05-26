namespace FinTrack.Domain.Common;

// Interface própria do domínio para não acoplar a MediatR.
// Na Application, os eventos são adaptados a INotification.
public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
