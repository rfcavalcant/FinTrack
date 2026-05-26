using FinTrack.Application.Common.Interfaces;

namespace FinTrack.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly FinTrackDbContext _dbContext;

    public UnitOfWork(FinTrackDbContext dbContext) => _dbContext = dbContext;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
