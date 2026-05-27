using FinTrack.Application.Common;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Accounts;
using FinTrack.Domain.Common;
using FinTrack.Domain.Transactions;

namespace FinTrack.Application.Accounts;

public sealed record DeleteAccountCommand(Guid Id);

public sealed class DeleteAccountCommandHandler : ICommandHandler<DeleteAccountCommand>
{
    private readonly IAccountRepository _accounts;
    private readonly ITransactionRepository _transactions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteAccountCommandHandler(
        IAccountRepository accounts,
        ITransactionRepository transactions,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _accounts = accounts;
        _transactions = transactions;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task HandleAsync(
        DeleteAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        var account = await _accounts.GetByIdAsync(command.Id, cancellationToken);
        if (account is null || account.UserId != _currentUser.UserId)
            throw new NotFoundException("Account not found.");

        if (await _transactions.ExistsForAccountAsync(account.Id, cancellationToken))
            throw new DomainException("Cannot delete an account that has transactions.");

        _accounts.Remove(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
