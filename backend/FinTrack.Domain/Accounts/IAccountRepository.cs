namespace FinTrack.Domain.Accounts;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Account>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(Account account);

    void Remove(Account account);
}
