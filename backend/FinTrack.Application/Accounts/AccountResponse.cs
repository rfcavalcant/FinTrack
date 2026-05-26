using FinTrack.Domain.Accounts;

namespace FinTrack.Application.Accounts;

public sealed record AccountResponse(
    Guid Id,
    string Name,
    string Type,
    decimal Balance,
    string Currency,
    decimal? CreditLimit)
{
    public static AccountResponse From(Account account) => new(
        account.Id,
        account.Name,
        account.Type.ToString(),
        account.Balance.Amount,
        account.Balance.Currency,
        account.CreditLimit?.Amount);
}
