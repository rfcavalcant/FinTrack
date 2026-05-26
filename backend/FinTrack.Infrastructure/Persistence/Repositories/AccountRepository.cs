using FinTrack.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly FinTrackDbContext _dbContext;

    public AccountRepository(FinTrackDbContext dbContext) => _dbContext = dbContext;

    public Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Account>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _dbContext.Accounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

    public void Add(Account account) => _dbContext.Accounts.Add(account);

    public void Remove(Account account) => _dbContext.Accounts.Remove(account);
}
