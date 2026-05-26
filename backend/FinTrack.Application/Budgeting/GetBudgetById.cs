using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Budgeting;
using MediatR;

namespace FinTrack.Application.Budgeting;

public sealed record GetBudgetByIdQuery(Guid Id) : IRequest<BudgetResponse>;

public sealed class GetBudgetByIdQueryHandler : IRequestHandler<GetBudgetByIdQuery, BudgetResponse>
{
    private readonly IBudgetRepository _budgets;
    private readonly ICurrentUserService _currentUser;

    public GetBudgetByIdQueryHandler(IBudgetRepository budgets, ICurrentUserService currentUser)
    {
        _budgets = budgets;
        _currentUser = currentUser;
    }

    public async Task<BudgetResponse> Handle(GetBudgetByIdQuery request, CancellationToken cancellationToken)
    {
        var budget = await _budgets.GetByIdAsync(request.Id, cancellationToken);
        if (budget is null || budget.UserId != _currentUser.UserId)
            throw new NotFoundException("Budget not found.");

        return BudgetResponse.From(budget);
    }
}
