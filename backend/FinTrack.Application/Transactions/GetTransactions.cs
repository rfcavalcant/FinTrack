using FinTrack.Application.Common;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Transactions;

namespace FinTrack.Application.Transactions;

public sealed record GetTransactionsQuery(
    DateOnly? From,
    DateOnly? To,
    Guid? CategoryId,
    Guid? AccountId);

public sealed class GetTransactionsQueryHandler
    : IQueryHandler<GetTransactionsQuery, IReadOnlyList<TransactionResponse>>
{
    private readonly ITransactionRepository _transactions;
    private readonly ICurrentUserService _currentUser;

    public GetTransactionsQueryHandler(ITransactionRepository transactions, ICurrentUserService currentUser)
    {
        _transactions = transactions;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TransactionResponse>> HandleAsync(
        GetTransactionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var filter = new TransactionFilter(query.From, query.To, query.CategoryId, query.AccountId);
        var transactions = await _transactions.GetByUserAsync(_currentUser.UserId, filter, cancellationToken);
        return transactions.Select(TransactionResponse.From).ToList();
    }
}
