using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Budgeting;
using FinTrack.Domain.Transactions;

namespace FinTrack.Application.Budgeting;

// Reage ao domain event TransactionRegistered para atualizar o consumo do orçamento.
// Consistência eventual: o commit da Transaction já ocorreu; esta é uma reação downstream.
// Ausência de orçamento é o caso normal — retorno silencioso sem erro.
public sealed class OnTransactionRegisteredHandler : IDomainEventHandler<TransactionRegistered>
{
    private readonly IBudgetRepository _budgets;
    private readonly IUnitOfWork _unitOfWork;

    public OnTransactionRegisteredHandler(IBudgetRepository budgets, IUnitOfWork unitOfWork)
    {
        _budgets = budgets;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        TransactionRegistered domainEvent,
        CancellationToken cancellationToken = default)
    {
        if (domainEvent.Type != TransactionType.Expense)
            return;

        var budget = await _budgets.FindActiveAsync(
            domainEvent.UserId, domainEvent.CategoryId, domainEvent.Date, cancellationToken);

        if (budget is null)
            return;

        budget.RegisterConsumption(domainEvent.Amount);

        // UnitOfWork coleta BudgetExceeded do agregado (se levantado) e o despacha
        // automaticamente pós-commit, sem nenhum código extra aqui.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
