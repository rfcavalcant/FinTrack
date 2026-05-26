using FinTrack.Domain.Common;
using MediatR;

namespace FinTrack.Application.Common;

public sealed record DomainEventNotification<T>(T Event) : INotification
    where T : IDomainEvent;
