using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Accounts;
using MediatR;

namespace FinTrack.Application.Accounts;

public sealed record GetAccountsQuery : IRequest<IReadOnlyList<AccountResponse>>;

public sealed class GetAccountsQueryHandler
    : IRequestHandler<GetAccountsQuery, IReadOnlyList<AccountResponse>>
{
    private readonly IAccountRepository _accounts;
    private readonly ICurrentUserService _currentUser;

    public GetAccountsQueryHandler(IAccountRepository accounts, ICurrentUserService currentUser)
    {
        _accounts = accounts;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<AccountResponse>> Handle(
        GetAccountsQuery request,
        CancellationToken cancellationToken)
    {
        var accounts = await _accounts.GetByUserAsync(_currentUser.UserId, cancellationToken);
        return accounts.Select(AccountResponse.From).ToList();
    }
}
