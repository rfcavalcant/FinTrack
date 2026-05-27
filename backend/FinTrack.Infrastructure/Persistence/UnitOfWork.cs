using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Common;

namespace FinTrack.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly FinTrackDbContext _dbContext;
    private readonly IDomainEventDispatcher _dispatcher;

    public UnitOfWork(FinTrackDbContext dbContext, IDomainEventDispatcher dispatcher)
    {
        _dbContext = dbContext;
        _dispatcher = dispatcher;
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
            await _dispatcher.DispatchAsync(domainEvent, cancellationToken);

        return result;
    }
}
