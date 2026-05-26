namespace FinTrack.Domain.Transactions;

// Critérios opcionais para consulta de lançamentos de um usuário.
public sealed record TransactionFilter(
    DateOnly? From = null,
    DateOnly? To = null,
    Guid? CategoryId = null,
    Guid? AccountId = null);
