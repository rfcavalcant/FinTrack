using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Accounts;
using FluentValidation;
using MediatR;

namespace FinTrack.Application.Accounts;

public sealed record RenameAccountCommand(Guid Id, string Name) : IRequest<AccountResponse>;

public sealed class RenameAccountCommandValidator : AbstractValidator<RenameAccountCommand>
{
    public RenameAccountCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public sealed class RenameAccountCommandHandler : IRequestHandler<RenameAccountCommand, AccountResponse>
{
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RenameAccountCommandHandler(
        IAccountRepository accounts,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _accounts = accounts;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<AccountResponse> Handle(RenameAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _accounts.GetByIdAsync(request.Id, cancellationToken);
        if (account is null || account.UserId != _currentUser.UserId)
        {
            throw new NotFoundException("Account not found.");
        }

        account.Rename(request.Name);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountResponse.From(account);
    }
}
