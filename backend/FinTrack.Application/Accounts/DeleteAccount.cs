using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Accounts;
using FinTrack.Domain.Common;
using FinTrack.Domain.Transactions;
using MediatR;

namespace FinTrack.Application.Accounts;

public sealed record DeleteAccountCommand(Guid Id) : IRequest;

public sealed class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
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

    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _accounts.GetByIdAsync(request.Id, cancellationToken);
        if (account is null || account.UserId != _currentUser.UserId)
        {
            throw new NotFoundException("Account not found.");
        }

        if (await _transactions.ExistsForAccountAsync(account.Id, cancellationToken))
        {
            throw new DomainException("Cannot delete an account that has transactions.");
        }

        _accounts.Remove(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
