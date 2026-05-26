namespace FinTrack.Application.Common.Exceptions;

// Recurso inexistente ou fora do escopo do usuário atual — mapeado para 404 na API.
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
