using FinTrack.Application.Common;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Accounts;

namespace FinTrack.Application.Accounts;

public sealed record GetAccountByIdQuery(Guid Id);

public sealed class GetAccountByIdQueryHandler : IQueryHandler<GetAccountByIdQuery, AccountResponse>
{
    private readonly IAccountRepository _accounts;
    private readonly ICurrentUserService _currentUser;

    public GetAccountByIdQueryHandler(IAccountRepository accounts, ICurrentUserService currentUser)
    {
        _accounts = accounts;
        _currentUser = currentUser;
    }

    public async Task<AccountResponse> HandleAsync(
        GetAccountByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var account = await _accounts.GetByIdAsync(query.Id, cancellationToken);
        if (account is null || account.UserId != _currentUser.UserId)
            throw new NotFoundException("Account not found.");

        return AccountResponse.From(account);
    }
}
