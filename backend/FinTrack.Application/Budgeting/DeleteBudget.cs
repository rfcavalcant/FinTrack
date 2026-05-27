using FinTrack.Application.Common;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Budgeting;

namespace FinTrack.Application.Budgeting;

public sealed record DeleteBudgetCommand(Guid Id);

public sealed class DeleteBudgetCommandHandler : ICommandHandler<DeleteBudgetCommand>
{
    private readonly IBudgetRepository _budgets;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteBudgetCommandHandler(
        IBudgetRepository budgets,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _budgets = budgets;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task HandleAsync(
        DeleteBudgetCommand command,
        CancellationToken cancellationToken = default)
    {
        var budget = await _budgets.GetByIdAsync(command.Id, cancellationToken);
        if (budget is null || budget.UserId != _currentUser.UserId)
            throw new NotFoundException("Budget not found.");

        _budgets.Remove(budget);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
