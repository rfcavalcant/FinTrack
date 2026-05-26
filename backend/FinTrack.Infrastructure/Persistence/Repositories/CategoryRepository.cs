using FinTrack.Domain.Categories;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly FinTrackDbContext _dbContext;

    public CategoryRepository(FinTrackDbContext dbContext) => _dbContext = dbContext;

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Category>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _dbContext.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public void Add(Category category) => _dbContext.Categories.Add(category);

    public void Remove(Category category) => _dbContext.Categories.Remove(category);
}
