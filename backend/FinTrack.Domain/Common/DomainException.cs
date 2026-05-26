namespace FinTrack.Domain.Common;

// Exceção base para violações de invariantes de domínio.
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
