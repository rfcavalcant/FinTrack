using FinTrack.Application.Common;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Accounts;
using FinTrack.Domain.Common;
using FluentValidation;

namespace FinTrack.Application.Accounts;

public sealed record OpenAccountCommand(
    string Name,
    AccountType Type,
    decimal InitialBalance,
    string? Currency,
    decimal? CreditLimit);

public sealed class OpenAccountCommandValidator : AbstractValidator<OpenAccountCommand>
{
    public OpenAccountCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.CreditLimit).GreaterThan(0).When(x => x.CreditLimit.HasValue);
    }
}

public sealed class OpenAccountCommandHandler : ICommandHandler<OpenAccountCommand, AccountResponse>
{
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public OpenAccountCommandHandler(
        IAccountRepository accounts,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _accounts = accounts;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<AccountResponse> HandleAsync(
        OpenAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        var currency = string.IsNullOrWhiteSpace(command.Currency) ? Money.DefaultCurrency : command.Currency!;
        var initialBalance = Money.Of(command.InitialBalance, currency);
        var creditLimit = command.CreditLimit.HasValue ? Money.Of(command.CreditLimit.Value, currency) : null;

        var account = Account.Open(_currentUser.UserId, command.Name, command.Type, initialBalance, creditLimit);

        _accounts.Add(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountResponse.From(account);
    }
}
