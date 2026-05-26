using FinTrack.Application.Common;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Budgeting;
using FinTrack.Domain.Transactions;
using MediatR;

namespace FinTrack.Application.Budgeting;

// Reage ao domain event TransactionRegistered para atualizar o consumo do orçamento.
// Consistência eventual: o commit da Transaction já ocorreu; esta é uma reação downstream.
// Ausência de orçamento é o caso normal — retorno silencioso sem erro.
public sealed class OnTransactionRegisteredHandler
    : INotificationHandler<DomainEventNotification<TransactionRegistered>>
{
    private readonly IBudgetRepository _budgets;
    private readonly IUnitOfWork _unitOfWork;

    public OnTransactionRegisteredHandler(IBudgetRepository budgets, IUnitOfWork unitOfWork)
    {
        _budgets = budgets;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DomainEventNotification<TransactionRegistered> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.Event;

        if (evt.Type != TransactionType.Expense)
            return;

        var budget = await _budgets.FindActiveAsync(
            evt.UserId, evt.CategoryId, evt.Date, cancellationToken);

        if (budget is null)
            return;

        budget.RegisterConsumption(evt.Amount);

        // UnitOfWork coleta BudgetExceeded do agregado (se levantado) e o despacha
        // automaticamente pós-commit, sem nenhum código extra aqui.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
