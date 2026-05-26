using FinTrack.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly FinTrackDbContext _dbContext;

    public UserRepository(FinTrackDbContext dbContext) => _dbContext = dbContext;

    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
        => _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
        => _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public void Add(User user) => _dbContext.Users.Add(user);
}
