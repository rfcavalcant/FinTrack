using FinTrack.Application.Common;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Common;
using MediatR;

namespace FinTrack.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly FinTrackDbContext _dbContext;
    private readonly IPublisher _publisher;

    public UnitOfWork(FinTrackDbContext dbContext, IPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Snapshot antes do commit. Se SaveChanges falhar, os eventos permanecem
        // nos agregados para uma possível nova tentativa.
        var domainEvents = _dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        var result = await _dbContext.SaveChangesAsync(cancellationToken);

        // Só limpa e despacha após o commit bem-sucedido.
        foreach (var entry in _dbContext.ChangeTracker.Entries<AggregateRoot>())
            entry.Entity.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>)
                .MakeGenericType(domainEvent.GetType());

            var notification = Activator.CreateInstance(notificationType, domainEvent)!;
            await _publisher.Publish(notification, cancellationToken);
        }

        return result;
    }
}
