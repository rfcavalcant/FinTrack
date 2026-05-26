namespace FinTrack.Domain.Identity;

// Repositório do agregado User. Métodos expressam intenção, sem vazar IQueryable.
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

    void Add(User user);
}
