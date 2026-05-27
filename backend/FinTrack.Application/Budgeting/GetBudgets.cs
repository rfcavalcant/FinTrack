using FinTrack.Application.Common;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Budgeting;

namespace FinTrack.Application.Budgeting;

public sealed record GetBudgetsQuery(int? Year, int? Month);

public sealed class GetBudgetsQueryHandler : IQueryHandler<GetBudgetsQuery, IReadOnlyList<BudgetResponse>>
{
    private readonly IBudgetRepository _budgets;
    private readonly ICurrentUserService _currentUser;

    public GetBudgetsQueryHandler(IBudgetRepository budgets, ICurrentUserService currentUser)
    {
        _budgets = budgets;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<BudgetResponse>> HandleAsync(
        GetBudgetsQuery query,
        CancellationToken cancellationToken = default)
    {
        var budgets = await _budgets.GetByUserAsync(_currentUser.UserId, cancellationToken);

        if (query.Year.HasValue && query.Month.HasValue)
        {
            budgets = budgets
                .Where(b => b.Period.Start.Year == query.Year.Value
                         && b.Period.Start.Month == query.Month.Value)
                .ToList();
        }

        return budgets.Select(BudgetResponse.From).ToList();
    }
}
