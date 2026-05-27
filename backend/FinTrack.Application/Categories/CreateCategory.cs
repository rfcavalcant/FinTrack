using FinTrack.Application.Common;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Categories;
using FluentValidation;

namespace FinTrack.Application.Categories;

public sealed record CreateCategoryCommand(string Name, CategoryType Type, string? Color);

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Color).MaximumLength(20);
    }
}

public sealed class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, CategoryResponse>
{
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateCategoryCommandHandler(
        ICategoryRepository categories,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _categories = categories;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<CategoryResponse> HandleAsync(
        CreateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var category = Category.Create(_currentUser.UserId, command.Name, command.Type, command.Color ?? string.Empty);

        _categories.Add(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CategoryResponse.From(category);
    }
}
