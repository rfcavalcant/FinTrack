using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Transactions;
using MediatR;

namespace FinTrack.Application.Transactions;

public sealed record GetTransactionByIdQuery(Guid Id) : IRequest<TransactionResponse>;

public sealed class GetTransactionByIdQueryHandler
    : IRequestHandler<GetTransactionByIdQuery, TransactionResponse>
{
    private readonly ITransactionRepository _transactions;
    private readonly ICurrentUserService _currentUser;

    public GetTransactionByIdQueryHandler(ITransactionRepository transactions, ICurrentUserService currentUser)
    {
        _transactions = transactions;
        _currentUser = currentUser;
    }

    public async Task<TransactionResponse> Handle(
        GetTransactionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var transaction = await _transactions.GetByIdAsync(request.Id, cancellationToken);
        if (transaction is null || transaction.UserId != _currentUser.UserId)
        {
            throw new NotFoundException("Transaction not found.");
        }

        return TransactionResponse.From(transaction);
    }
}
