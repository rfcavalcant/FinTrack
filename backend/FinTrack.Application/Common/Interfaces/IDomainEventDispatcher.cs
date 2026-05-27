using FinTrack.Domain.Common;

namespace FinTrack.Application.Common.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
