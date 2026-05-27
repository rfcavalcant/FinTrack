using FinTrack.Application.Common;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Accounts;
using FinTrack.Domain.Categories;
using FinTrack.Domain.Common;
using FinTrack.Domain.Transactions;
using FluentValidation;

namespace FinTrack.Application.Transactions;

public sealed record RegisterTransactionCommand(
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    decimal Amount,
    DateOnly Date,
    string? Description);

public sealed class RegisterTransactionCommandValidator : AbstractValidator<RegisterTransactionCommand>
{
    public RegisterTransactionCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).MaximumLength(300);
    }
}

public sealed class RegisterTransactionCommandHandler
    : ICommandHandler<RegisterTransactionCommand, TransactionResponse>
{
    private readonly IAccountRepository _accounts;
    private readonly ICategoryRepository _categories;
    private readonly ITransactionRepository _transactions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RegisterTransactionCommandHandler(
        IAccountRepository accounts,
        ICategoryRepository categories,
        ITransactionRepository transactions,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _accounts = accounts;
        _categories = categories;
        _transactions = transactions;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<TransactionResponse> HandleAsync(
        RegisterTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;

        var account = await _accounts.GetByIdAsync(command.AccountId, cancellationToken);
        if (account is null || account.UserId != userId)
            throw new NotFoundException("Account not found.");

        var category = await _categories.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category is null || category.UserId != userId)
            throw new NotFoundException("Category not found.");

        var expectedCategoryType = command.Type == TransactionType.Income
            ? CategoryType.Income
            : CategoryType.Expense;
        if (category.Type != expectedCategoryType)
            throw new DomainException("Category type does not match the transaction type.");

        // Lançamento na moeda da conta — garante que Credit/Debit não rejeitem por moeda divergente.
        var amount = Money.Of(command.Amount, account.Balance.Currency);

        var transaction = command.Type == TransactionType.Income
            ? Transaction.RegisterIncome(userId, account.Id, category.Id, amount, command.Date, command.Description)
            : Transaction.RegisterExpense(userId, account.Id, category.Id, amount, command.Date, command.Description);

        // Atualização de saldo via domínio, na mesma unidade de trabalho (consistência forte).
        if (command.Type == TransactionType.Income)
            account.Credit(amount);
        else
            account.Debit(amount);

        _transactions.Add(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TransactionResponse.From(transaction);
    }
}
