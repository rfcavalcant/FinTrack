using FinTrack.Application.Budgeting;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Budgeting;
using FinTrack.Domain.Categories;
using FinTrack.Domain.Common;
using FluentAssertions;
using NSubstitute;

namespace FinTrack.Tests.Application.Budgeting;

public class DefineBudgetCommandHandlerTests
{
    private readonly IBudgetRepository _budgets = Substitute.For<IBudgetRepository>();
    private readonly ICategoryRepository _categories = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly DefineBudgetCommandHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();

    public DefineBudgetCommandHandlerTests()
    {
        _currentUser.UserId.Returns(UserId);
        _handler = new DefineBudgetCommandHandler(_budgets, _categories, _unitOfWork, _currentUser);
    }

    private void SetupExpenseCategory()
    {
        var category = Category.Create(UserId, "Alimentação", CategoryType.Expense, string.Empty);
        _categories.GetByIdAsync(CategoryId, Arg.Any<CancellationToken>()).Returns(category);
    }

    [Fact]
    public async Task Handle_WithValidData_CreatesBudgetAndPersists()
    {
        SetupExpenseCategory();
        _budgets.FindOverlappingAsync(UserId, CategoryId, Arg.Any<DateRange>(), Arg.Any<CancellationToken>())
            .Returns((Budget?)null);

        var result = await _handler.Handle(
            new DefineBudgetCommand(CategoryId, 2026, 5, 1000m, null), CancellationToken.None);

        result.LimitAmount.Should().Be(1000m);
        result.ConsumptionAmount.Should().Be(0m);
        result.Currency.Should().Be(Money.DefaultCurrency);
        result.PeriodStart.Should().Be(new DateOnly(2026, 5, 1));
        result.PeriodEnd.Should().Be(new DateOnly(2026, 5, 31));
        _budgets.Received(1).Add(Arg.Any<Budget>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithOverlappingBudget_ThrowsDomainException()
    {
        SetupExpenseCategory();
        var existing = Budget.Define(UserId, CategoryId,
            DateRange.ForMonth(2026, 5), Money.Of(500m));
        _budgets.FindOverlappingAsync(UserId, CategoryId, Arg.Any<DateRange>(), Arg.Any<CancellationToken>())
            .Returns(existing);

        await _handler.Invoking(h =>
                h.Handle(new DefineBudgetCommand(CategoryId, 2026, 5, 1000m, null), CancellationToken.None))
            .Should().ThrowAsync<DomainException>()
            .WithMessage("*already exists*");

        _budgets.DidNotReceive().Add(Arg.Any<Budget>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithIncomeCategory_ThrowsDomainException()
    {
        var incomeCategory = Category.Create(UserId, "Salário", CategoryType.Income, string.Empty);
        _categories.GetByIdAsync(CategoryId, Arg.Any<CancellationToken>()).Returns(incomeCategory);

        await _handler.Invoking(h =>
                h.Handle(new DefineBudgetCommand(CategoryId, 2026, 5, 1000m, null), CancellationToken.None))
            .Should().ThrowAsync<DomainException>();

        _budgets.DidNotReceive().Add(Arg.Any<Budget>());
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ThrowsNotFoundException()
    {
        _categories.GetByIdAsync(CategoryId, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        await _handler.Invoking(h =>
                h.Handle(new DefineBudgetCommand(CategoryId, 2026, 5, 1000m, null), CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithCustomCurrency_UsesThatCurrency()
    {
        SetupExpenseCategory();
        _budgets.FindOverlappingAsync(UserId, CategoryId, Arg.Any<DateRange>(), Arg.Any<CancellationToken>())
            .Returns((Budget?)null);

        var result = await _handler.Handle(
            new DefineBudgetCommand(CategoryId, 2026, 5, 500m, "USD"), CancellationToken.None);

        result.Currency.Should().Be("USD");
    }
}
