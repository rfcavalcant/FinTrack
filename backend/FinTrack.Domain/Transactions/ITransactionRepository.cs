namespace FinTrack.Domain.Transactions;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transaction>> GetByUserAsync(
        Guid userId,
        TransactionFilter filter,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsForAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

    Task<bool> ExistsForCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);

    void Add(Transaction transaction);

    void Remove(Transaction transaction);
}
