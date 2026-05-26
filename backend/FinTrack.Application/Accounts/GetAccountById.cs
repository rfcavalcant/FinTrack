using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Accounts;
using MediatR;

namespace FinTrack.Application.Accounts;

public sealed record GetAccountByIdQuery(Guid Id) : IRequest<AccountResponse>;

public sealed class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountResponse>
{
    private readonly IAccountRepository _accounts;
    private readonly ICurrentUserService _currentUser;

    public GetAccountByIdQueryHandler(IAccountRepository accounts, ICurrentUserService currentUser)
    {
        _accounts = accounts;
        _currentUser = currentUser;
    }

    public async Task<AccountResponse> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _accounts.GetByIdAsync(request.Id, cancellationToken);
        if (account is null || account.UserId != _currentUser.UserId)
        {
            throw new NotFoundException("Account not found.");
        }

        return AccountResponse.From(account);
    }
}
