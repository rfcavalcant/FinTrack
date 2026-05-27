using FinTrack.Application.Common;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Transactions;

namespace FinTrack.Application.Transactions;

public sealed record GetTransactionByIdQuery(Guid Id);

public sealed class GetTransactionByIdQueryHandler : IQueryHandler<GetTransactionByIdQuery, TransactionResponse>
{
    private readonly ITransactionRepository _transactions;
    private readonly ICurrentUserService _currentUser;

    public GetTransactionByIdQueryHandler(ITransactionRepository transactions, ICurrentUserService currentUser)
    {
        _transactions = transactions;
        _currentUser = currentUser;
    }

    public async Task<TransactionResponse> HandleAsync(
        GetTransactionByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactions.GetByIdAsync(query.Id, cancellationToken);
        if (transaction is null || transaction.UserId != _currentUser.UserId)
            throw new NotFoundException("Transaction not found.");

        return TransactionResponse.From(transaction);
    }
}
