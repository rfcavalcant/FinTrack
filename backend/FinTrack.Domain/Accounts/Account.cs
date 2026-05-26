using FinTrack.Domain.Common;

namespace FinTrack.Domain.Accounts;

// Raiz de agregado do contexto Ledger. O saldo SÓ muda por Credit/Debit.
public sealed class Account : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public AccountType Type { get; private set; }
    public Money Balance { get; private set; } = null!;
    public Money? CreditLimit { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Account()
    {
    }

    private Account(Guid id, Guid userId, string name, AccountType type, Money balance, Money? creditLimit)
        : base(id)
    {
        UserId = userId;
        Name = name;
        Type = type;
        Balance = balance;
        CreditLimit = creditLimit;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Account Open(
        Guid userId,
        string name,
        AccountType type,
        Money initialBalance,
        Money? creditLimit = null)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("Account must belong to a user.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Account name is required.");
        }

        if (type == AccountType.CreditCard)
        {
            if (creditLimit is null)
            {
                throw new DomainException("A credit card account requires a credit limit.");
            }

            if (creditLimit.IsNegative)
            {
                throw new DomainException("Credit limit cannot be negative.");
            }
        }
        else if (initialBalance.IsNegative)
        {
            throw new DomainException("Initial balance of a non-credit account cannot be negative.");
        }

        return new Account(Guid.NewGuid(), userId, name.Trim(), type, initialBalance, creditLimit);
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Account name is required.");
        }

        Name = name.Trim();
    }

    // Entrada de dinheiro (receita).
    public void Credit(Money amount)
    {
        EnsurePositive(amount);
        Balance = Balance.Add(amount);
    }

    // Saída de dinheiro (despesa). Cartão consome limite; demais não podem ficar negativos.
    public void Debit(Money amount)
    {
        EnsurePositive(amount);

        var resulting = Balance.Subtract(amount);

        if (Type == AccountType.CreditCard)
        {
            var floor = Money.Zero(Balance.Currency).Subtract(CreditLimit!);
            if (resulting.IsLessThan(floor))
            {
                throw new DomainException("Debit exceeds the available credit limit.");
            }
        }
        else if (resulting.IsNegative)
        {
            throw new DomainException("Insufficient balance.");
        }

        Balance = resulting;
    }

    // Estorno de um lançamento já aplicado (correção/exclusão). Não aplica a trava de
    // saldo insuficiente, pois é o desfazer de um movimento que já existiu — não um novo gasto.
    public void ReverseCredit(Money amount)
    {
        EnsurePositive(amount);
        Balance = Balance.Subtract(amount);
    }

    public void ReverseDebit(Money amount)
    {
        EnsurePositive(amount);
        Balance = Balance.Add(amount);
    }

    private static void EnsurePositive(Money amount)
    {
        if (amount.IsNegative || amount.IsZero)
        {
            throw new DomainException("Amount must be positive.");
        }
    }
}
