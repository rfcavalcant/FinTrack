using FinTrack.Domain.Transactions;

namespace FinTrack.Application.Transactions;

public sealed record TransactionResponse(
    Guid Id,
    Guid AccountId,
    Guid CategoryId,
    string Type,
    decimal Amount,
    string Currency,
    DateOnly Date,
    string? Description)
{
    public static TransactionResponse From(Transaction transaction) => new(
        transaction.Id,
        transaction.AccountId,
        transaction.CategoryId,
        transaction.Type.ToString(),
        transaction.Amount.Amount,
        transaction.Amount.Currency,
        transaction.Date,
        transaction.Description);
}
