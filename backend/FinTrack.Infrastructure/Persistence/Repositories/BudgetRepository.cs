using FinTrack.Domain.Budgeting;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence.Repositories;

public sealed class BudgetRepository : IBudgetRepository
{
    private readonly FinTrackDbContext _dbContext;

    public BudgetRepository(FinTrackDbContext dbContext) => _dbContext = dbContext;

    public Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Budgets.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Budget>> GetByUserAsync(
        Guid userId, CancellationToken cancellationToken = default)
        => await _dbContext.Budgets
            .Where(b => b.UserId == userId)
            .OrderBy(b => b.Period.Start)
            .ThenBy(b => b.CategoryId)
            .ToListAsync(cancellationToken);

    public Task<Budget?> FindActiveAsync(
        Guid userId, Guid categoryId, DateOnly date, CancellationToken cancellationToken = default)
        => _dbContext.Budgets.FirstOrDefaultAsync(
            b => b.UserId == userId
              && b.CategoryId == categoryId
              && b.Period.Start <= date
              && b.Period.End >= date,
            cancellationToken);

    // Verifica sobreposição pelo intervalo clássico: A.Start <= B.End && A.End >= B.Start.
    public Task<Budget?> FindOverlappingAsync(
        Guid userId, Guid categoryId, DateRange period, CancellationToken cancellationToken = default)
        => _dbContext.Budgets.FirstOrDefaultAsync(
            b => b.UserId == userId
              && b.CategoryId == categoryId
              && b.Period.Start <= period.End
              && b.Period.End >= period.Start,
            cancellationToken);

    public void Add(Budget budget) => _dbContext.Budgets.Add(budget);

    public void Remove(Budget budget) => _dbContext.Budgets.Remove(budget);
}
