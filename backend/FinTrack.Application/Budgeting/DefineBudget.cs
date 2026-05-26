using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Budgeting;
using FinTrack.Domain.Categories;
using FinTrack.Domain.Common;
using FluentValidation;
using MediatR;

namespace FinTrack.Application.Budgeting;

public sealed record DefineBudgetCommand(
    Guid CategoryId,
    int Year,
    int Month,
    decimal LimitAmount,
    string? Currency) : IRequest<BudgetResponse>;

public sealed class DefineBudgetCommandValidator : AbstractValidator<DefineBudgetCommand>
{
    public DefineBudgetCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.LimitAmount).GreaterThan(0);
    }
}

public sealed class DefineBudgetCommandHandler : IRequestHandler<DefineBudgetCommand, BudgetResponse>
{
    private readonly IBudgetRepository _budgets;
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DefineBudgetCommandHandler(
        IBudgetRepository budgets,
        ICategoryRepository categories,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _budgets = budgets;
        _categories = categories;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<BudgetResponse> Handle(DefineBudgetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var category = await _categories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null || category.UserId != userId)
            throw new NotFoundException("Category not found.");

        if (category.Type != CategoryType.Expense)
            throw new DomainException("Budgets can only be defined for expense categories.");

        var period = DateRange.ForMonth(request.Year, request.Month);

        var overlapping = await _budgets.FindOverlappingAsync(
            userId, request.CategoryId, period, cancellationToken);

        if (overlapping is not null)
            throw new DomainException("A budget for this category already exists in the specified period.");

        var currency = string.IsNullOrWhiteSpace(request.Currency)
            ? Money.DefaultCurrency
            : request.Currency!;

        var budget = Budget.Define(userId, request.CategoryId, period, Money.Of(request.LimitAmount, currency));

        _budgets.Add(budget);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BudgetResponse.From(budget);
    }
}
