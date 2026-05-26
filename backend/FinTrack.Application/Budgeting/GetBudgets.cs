using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Budgeting;
using MediatR;

namespace FinTrack.Application.Budgeting;

public sealed record GetBudgetsQuery(int? Year, int? Month) : IRequest<IReadOnlyList<BudgetResponse>>;

public sealed class GetBudgetsQueryHandler : IRequestHandler<GetBudgetsQuery, IReadOnlyList<BudgetResponse>>
{
    private readonly IBudgetRepository _budgets;
    private readonly ICurrentUserService _currentUser;

    public GetBudgetsQueryHandler(IBudgetRepository budgets, ICurrentUserService currentUser)
    {
        _budgets = budgets;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<BudgetResponse>> Handle(
        GetBudgetsQuery request, CancellationToken cancellationToken)
    {
        var budgets = await _budgets.GetByUserAsync(_currentUser.UserId, cancellationToken);

        if (request.Year.HasValue && request.Month.HasValue)
        {
            budgets = budgets
                .Where(b => b.Period.Start.Year == request.Year.Value
                         && b.Period.Start.Month == request.Month.Value)
                .ToList();
        }

        return budgets.Select(BudgetResponse.From).ToList();
    }
}
