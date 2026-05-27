using FinTrack.Application.Common;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Budgeting;

namespace FinTrack.Application.Budgeting;

public sealed record GetBudgetByIdQuery(Guid Id);

public sealed class GetBudgetByIdQueryHandler : IQueryHandler<GetBudgetByIdQuery, BudgetResponse>
{
    private readonly IBudgetRepository _budgets;
    private readonly ICurrentUserService _currentUser;

    public GetBudgetByIdQueryHandler(IBudgetRepository budgets, ICurrentUserService currentUser)
    {
        _budgets = budgets;
        _currentUser = currentUser;
    }

    public async Task<BudgetResponse> HandleAsync(
        GetBudgetByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var budget = await _budgets.GetByIdAsync(query.Id, cancellationToken);
        if (budget is null || budget.UserId != _currentUser.UserId)
            throw new NotFoundException("Budget not found.");

        return BudgetResponse.From(budget);
    }
}
