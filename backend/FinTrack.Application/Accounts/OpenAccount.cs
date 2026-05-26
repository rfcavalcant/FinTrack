using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Accounts;
using FinTrack.Domain.Common;
using FluentValidation;
using MediatR;

namespace FinTrack.Application.Accounts;

public sealed record OpenAccountCommand(
    string Name,
    AccountType Type,
    decimal InitialBalance,
    string? Currency,
    decimal? CreditLimit) : IRequest<AccountResponse>;

public sealed class OpenAccountCommandValidator : AbstractValidator<OpenAccountCommand>
{
    public OpenAccountCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.CreditLimit).GreaterThan(0).When(x => x.CreditLimit.HasValue);
    }
}

public sealed class OpenAccountCommandHandler : IRequestHandler<OpenAccountCommand, AccountResponse>
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

    public async Task<AccountResponse> Handle(OpenAccountCommand request, CancellationToken cancellationToken)
    {
        var currency = string.IsNullOrWhiteSpace(request.Currency) ? Money.DefaultCurrency : request.Currency!;
        var initialBalance = Money.Of(request.InitialBalance, currency);
        var creditLimit = request.CreditLimit.HasValue ? Money.Of(request.CreditLimit.Value, currency) : null;

        var account = Account.Open(_currentUser.UserId, request.Name, request.Type, initialBalance, creditLimit);

        _accounts.Add(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountResponse.From(account);
    }
}
