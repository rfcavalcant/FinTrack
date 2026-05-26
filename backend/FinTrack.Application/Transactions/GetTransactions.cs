using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Transactions;
using MediatR;

namespace FinTrack.Application.Transactions;

public sealed record GetTransactionsQuery(
    DateOnly? From,
    DateOnly? To,
    Guid? CategoryId,
    Guid? AccountId) : IRequest<IReadOnlyList<TransactionResponse>>;

public sealed class GetTransactionsQueryHandler
    : IRequestHandler<GetTransactionsQuery, IReadOnlyList<TransactionResponse>>
{
    private readonly ITransactionRepository _transactions;
    private readonly ICurrentUserService _currentUser;

    public GetTransactionsQueryHandler(ITransactionRepository transactions, ICurrentUserService currentUser)
    {
        _transactions = transactions;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TransactionResponse>> Handle(
        GetTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var filter = new TransactionFilter(request.From, request.To, request.CategoryId, request.AccountId);
        var transactions = await _transactions.GetByUserAsync(_currentUser.UserId, filter, cancellationToken);
        return transactions.Select(TransactionResponse.From).ToList();
    }
}
