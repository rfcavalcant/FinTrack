namespace FinTrack.Domain.Budgeting;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Budget>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    // Encontra o orçamento que cobre uma data específica (usado pelo handler de evento).
    Task<Budget?> FindActiveAsync(
        Guid userId, Guid categoryId, DateOnly date, CancellationToken cancellationToken = default);

    // Detecta sobreposição de período para impedir duplicatas (usado pelo command de criação).
    Task<Budget?> FindOverlappingAsync(
        Guid userId, Guid categoryId, DateRange period, CancellationToken cancellationToken = default);

    void Add(Budget budget);

    void Remove(Budget budget);
}
