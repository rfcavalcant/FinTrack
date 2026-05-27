using FinTrack.Application.Common;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Accounts;

namespace FinTrack.Application.Accounts;

public sealed record GetAccountsQuery;

public sealed class GetAccountsQueryHandler : IQueryHandler<GetAccountsQuery, IReadOnlyList<AccountResponse>>
{
    private readonly IAccountRepository _accounts;
    private readonly ICurrentUserService _currentUser;

    public GetAccountsQueryHandler(IAccountRepository accounts, ICurrentUserService currentUser)
    {
        _accounts = accounts;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<AccountResponse>> HandleAsync(
        GetAccountsQuery query,
        CancellationToken cancellationToken = default)
    {
        var accounts = await _accounts.GetByUserAsync(_currentUser.UserId, cancellationToken);
        return accounts.Select(AccountResponse.From).ToList();
    }
}
