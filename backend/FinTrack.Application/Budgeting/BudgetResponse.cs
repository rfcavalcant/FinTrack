using FinTrack.Domain.Budgeting;

namespace FinTrack.Application.Budgeting;

public sealed record BudgetResponse(
    Guid Id,
    Guid CategoryId,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal LimitAmount,
    string Currency,
    decimal ConsumptionAmount,
    bool IsExceeded)
{
    public static BudgetResponse From(Budget budget) => new(
        budget.Id,
        budget.CategoryId,
        budget.Period.Start,
        budget.Period.End,
        budget.Limit.Amount,
        budget.Limit.Currency,
        budget.Consumption.Amount,
        budget.IsExceeded);
}
