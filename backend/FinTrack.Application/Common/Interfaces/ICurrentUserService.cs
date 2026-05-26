namespace FinTrack.Application.Common.Interfaces;

// Usuário autenticado da requisição atual. UserId vem do token, nunca do corpo da requisição.
public interface ICurrentUserService
{
    Guid UserId { get; }
}
