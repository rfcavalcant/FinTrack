using FinTrack.Domain.Common;

namespace FinTrack.Domain.Goals;

// Raiz de agregado do contexto Goals. Objetivo de acumular um valor até um prazo.
public sealed class Goal : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Money Target { get; private set; } = null!;
    public Money CurrentAmount { get; private set; } = null!;
    public DateOnly? Deadline { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Goal()
    {
    }

    private Goal(Guid id, Guid userId, string name, Money target, DateOnly? deadline) : base(id)
    {
        UserId = userId;
        Name = name;
        Target = target;
        CurrentAmount = Money.Zero(target.Currency);
        Deadline = deadline;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Goal Define(Guid userId, string name, Money target, DateOnly? deadline = null)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("Goal must belong to a user.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Goal name is required.");
        }

        if (target.IsNegative || target.IsZero)
        {
            throw new DomainException("Goal target must be positive.");
        }

        return new Goal(Guid.NewGuid(), userId, name.Trim(), target, deadline);
    }

    // Aporte. Não aceita valor negativo/zero; ao atingir o alvo, levanta GoalReached.
    public void Contribute(Money amount)
    {
        if (amount.IsNegative || amount.IsZero)
        {
            throw new DomainException("Contribution must be positive.");
        }

        var wasReached = CurrentAmount.IsGreaterThanOrEqualTo(Target);
        CurrentAmount = CurrentAmount.Add(amount);

        if (!wasReached && CurrentAmount.IsGreaterThanOrEqualTo(Target))
        {
            RaiseDomainEvent(new GoalReached(Id, UserId, Target, CurrentAmount));
        }
    }

    public bool IsReached => CurrentAmount.IsGreaterThanOrEqualTo(Target);
}
