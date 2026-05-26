using FinTrack.Domain.Common;

namespace FinTrack.Domain.Identity;

// Raiz de agregado do contexto Identity.
public sealed class User : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }

    private User()
    {
    }

    private User(Guid id, string name, Email email, string passwordHash) : base(id)
    {
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static User Register(string name, Email email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("User name is required.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Password hash is required.");
        }

        return new User(Guid.NewGuid(), name.Trim(), email, passwordHash);
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("User name is required.");
        }

        Name = name.Trim();
    }
}
