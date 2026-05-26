using FinTrack.Domain.Accounts;
using FinTrack.Domain.Categories;
using FinTrack.Domain.Transactions;

namespace FinTrack.API.Contracts;

public sealed record OpenAccountRequest(
    string Name,
    AccountType Type,
    decimal InitialBalance,
    string? Currency,
    decimal? CreditLimit);

public sealed record RenameAccountRequest(string Name);

public sealed record CreateCategoryRequest(string Name, CategoryType Type, string? Color);

public sealed record UpdateCategoryRequest(string Name, string? Color);

public sealed record RegisterTransactionRequest(
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    decimal Amount,
    DateOnly Date,
    string? Description);

public sealed record DefineBudgetRequest(
    Guid CategoryId,
    int Year,
    int Month,
    decimal LimitAmount,
    string? Currency);
