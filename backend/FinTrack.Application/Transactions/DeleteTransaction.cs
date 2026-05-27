using FinTrack.Application.Common;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Accounts;
using FinTrack.Domain.Transactions;

namespace FinTrack.Application.Transactions;

public sealed record DeleteTransactionCommand(Guid Id);

public sealed class DeleteTransactionCommandHandler : ICommandHandler<DeleteTransactionCommand>
{
    private readonly ITransactionRepository _transactions;
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteTransactionCommandHandler(
        ITransactionRepository transactions,
        IAccountRepository accounts,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _transactions = transactions;
        _accounts = accounts;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task HandleAsync(
        DeleteTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactions.GetByIdAsync(command.Id, cancellationToken);
        if (transaction is null || transaction.UserId != _currentUser.UserId)
            throw new NotFoundException("Transaction not found.");

        // Estorna o efeito no saldo da conta (mesma unidade de trabalho).
        var account = await _accounts.GetByIdAsync(transaction.AccountId, cancellationToken);
        if (account is not null)
        {
            if (transaction.Type == TransactionType.Income)
                account.ReverseCredit(transaction.Amount);
            else
                account.ReverseDebit(transaction.Amount);
        }

        _transactions.Remove(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
