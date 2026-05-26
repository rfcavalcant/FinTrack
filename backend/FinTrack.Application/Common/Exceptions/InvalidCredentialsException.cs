namespace FinTrack.Application.Common.Exceptions;

// Falha de autenticação (email inexistente ou senha incorreta) — mapeada para 401 na API.
// Mensagem genérica de propósito, para não revelar se o email existe.
public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Invalid email or password.")
    {
    }
}
