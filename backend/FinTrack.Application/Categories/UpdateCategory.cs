using FinTrack.Application.Common;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Categories;
using FluentValidation;

namespace FinTrack.Application.Categories;

public sealed record UpdateCategoryCommand(Guid Id, string Name, string? Color);

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Color).MaximumLength(20);
    }
}

public sealed class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand, CategoryResponse>
{
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categories,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _categories = categories;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<CategoryResponse> HandleAsync(
        UpdateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdAsync(command.Id, cancellationToken);
        if (category is null || category.UserId != _currentUser.UserId)
            throw new NotFoundException("Category not found.");

        category.Rename(command.Name);
        category.ChangeColor(command.Color ?? string.Empty);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CategoryResponse.From(category);
    }
}
