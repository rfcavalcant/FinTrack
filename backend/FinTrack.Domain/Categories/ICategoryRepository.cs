namespace FinTrack.Domain.Categories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(Category category);

    void Remove(Category category);
}
