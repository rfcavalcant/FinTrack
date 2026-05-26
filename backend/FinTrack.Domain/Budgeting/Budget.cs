using FinTrack.Domain.Common;

namespace FinTrack.Domain.Budgeting;

// Raiz de agregado do contexto Budgeting. Limite de gasto de uma categoria num período.
public sealed class Budget : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public DateRange Period { get; private set; } = null!;
    public Money Limit { get; private set; } = null!;
    public Money Consumption { get; private set; } = null!;

    private Budget()
    {
    }

    private Budget(Guid id, Guid userId, Guid categoryId, DateRange period, Money limit) : base(id)
    {
        UserId = userId;
        CategoryId = categoryId;
        Period = period;
        Limit = limit;
        Consumption = Money.Zero(limit.Currency);
    }

    public static Budget Define(Guid userId, Guid categoryId, DateRange period, Money limit)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("Budget must belong to a user.");
        }

        if (categoryId == Guid.Empty)
        {
            throw new DomainException("Budget must reference a category.");
        }

        if (limit.IsNegative || limit.IsZero)
        {
            throw new DomainException("Budget limit must be positive.");
        }

        return new Budget(Guid.NewGuid(), userId, categoryId, period, limit);
    }

    // Acumula consumo; ao ultrapassar o limite, levanta BudgetExceeded.
    public void RegisterConsumption(Money amount)
    {
        if (amount.IsNegative)
        {
            throw new DomainException("Consumption cannot be negative.");
        }

        Consumption = Consumption.Add(amount);

        if (Consumption.IsGreaterThan(Limit))
        {
            RaiseDomainEvent(new BudgetExceeded(Id, UserId, CategoryId, Limit, Consumption));
        }
    }

    public bool IsExceeded => Consumption.IsGreaterThan(Limit);
}
