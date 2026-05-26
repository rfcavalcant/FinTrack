using FinTrack.Domain.Common;

namespace FinTrack.Domain.Identity;

// Violação da unicidade de email — mapeada para 409 Conflict na API.
public sealed class DuplicateEmailException : DomainException
{
    public DuplicateEmailException(string email)
        : base($"Email '{email}' is already registered.")
    {
    }
}
