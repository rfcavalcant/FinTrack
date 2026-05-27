using FinTrack.Application.Common;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Accounts;
using FluentValidation;

namespace FinTrack.Application.Accounts;

public sealed record RenameAccountCommand(Guid Id, string Name);

public sealed class RenameAccountCommandValidator : AbstractValidator<RenameAccountCommand>
{
    public RenameAccountCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public sealed class RenameAccountCommandHandler : ICommandHandler<RenameAccountCommand, AccountResponse>
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

    public async Task<AccountResponse> HandleAsync(
        RenameAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        var account = await _accounts.GetByIdAsync(command.Id, cancellationToken);
        if (account is null || account.UserId != _currentUser.UserId)
            throw new NotFoundException("Account not found.");

        account.Rename(command.Name);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountResponse.From(account);
    }
}
