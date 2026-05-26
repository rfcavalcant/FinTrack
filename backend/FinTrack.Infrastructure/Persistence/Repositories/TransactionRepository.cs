using FinTrack.Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly FinTrackDbContext _dbContext;

    public TransactionRepository(FinTrackDbContext dbContext) => _dbContext = dbContext;

    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Transaction>> GetByUserAsync(
        Guid userId,
        TransactionFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Transactions.Where(t => t.UserId == userId);

        if (filter.From.HasValue)
        {
            query = query.Where(t => t.Date >= filter.From.Value);
        }

        if (filter.To.HasValue)
        {
            query = query.Where(t => t.Date <= filter.To.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
        }

        if (filter.AccountId.HasValue)
        {
            query = query.Where(t => t.AccountId == filter.AccountId.Value);
        }

        return await query
            .OrderByDescending(t => t.Date)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsForAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
        => _dbContext.Transactions.AnyAsync(t => t.AccountId == accountId, cancellationToken);

    public Task<bool> ExistsForCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => _dbContext.Transactions.AnyAsync(t => t.CategoryId == categoryId, cancellationToken);

    public void Add(Transaction transaction) => _dbContext.Transactions.Add(transaction);

    public void Remove(Transaction transaction) => _dbContext.Transactions.Remove(transaction);
}
