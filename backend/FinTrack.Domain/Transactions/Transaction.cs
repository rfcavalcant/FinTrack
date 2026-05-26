using FinTrack.Domain.Common;

namespace FinTrack.Domain.Transactions;

// Raiz de agregado do contexto Ledger. Referencia Account e Category por Id.
public sealed class Transaction : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid CategoryId { get; private set; }
    public TransactionType Type { get; private set; }
    public Money Amount { get; private set; } = null!;
    public DateOnly Date { get; private set; }
    public string? Description { get; private set; }

    private Transaction()
    {
    }

    private Transaction(
        Guid id,
        Guid userId,
        Guid accountId,
        Guid categoryId,
        TransactionType type,
        Money amount,
        DateOnly date,
        string? description) : base(id)
    {
        UserId = userId;
        AccountId = accountId;
        CategoryId = categoryId;
        Type = type;
        Amount = amount;
        Date = date;
        Description = description;
    }

    public static Transaction RegisterIncome(
        Guid userId,
        Guid accountId,
        Guid categoryId,
        Money amount,
        DateOnly date,
        string? description = null)
        => Register(TransactionType.Income, userId, accountId, categoryId, amount, date, description);

    public static Transaction RegisterExpense(
        Guid userId,
        Guid accountId,
        Guid categoryId,
        Money amount,
        DateOnly date,
        string? description = null)
        => Register(TransactionType.Expense, userId, accountId, categoryId, amount, date, description);

    private static Transaction Register(
        TransactionType type,
        Guid userId,
        Guid accountId,
        Guid categoryId,
        Money amount,
        DateOnly date,
        string? description)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("Transaction must belong to a user.");
        }

        if (accountId == Guid.Empty)
        {
            throw new DomainException("Transaction must reference an account.");
        }

        if (categoryId == Guid.Empty)
        {
            throw new DomainException("Transaction must reference a category.");
        }

        if (amount.IsNegative || amount.IsZero)
        {
            throw new DomainException("Transaction amount must be positive.");
        }

        var transaction = new Transaction(
            Guid.NewGuid(),
            userId,
            accountId,
            categoryId,
            type,
            amount,
            date,
            string.IsNullOrWhiteSpace(description) ? null : description.Trim());

        transaction.RaiseDomainEvent(new TransactionRegistered(
            transaction.Id,
            transaction.UserId,
            transaction.AccountId,
            transaction.CategoryId,
            transaction.Type,
            transaction.Amount,
            transaction.Date));

        return transaction;
    }
}
