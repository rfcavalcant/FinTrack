using FinTrack.Application.Common;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Categories;

namespace FinTrack.Application.Categories;

public sealed record GetCategoryByIdQuery(Guid Id);

public sealed class GetCategoryByIdQueryHandler : IQueryHandler<GetCategoryByIdQuery, CategoryResponse>
{
    private readonly ICategoryRepository _categories;
    private readonly ICurrentUserService _currentUser;

    public GetCategoryByIdQueryHandler(ICategoryRepository categories, ICurrentUserService currentUser)
    {
        _categories = categories;
        _currentUser = currentUser;
    }

    public async Task<CategoryResponse> HandleAsync(
        GetCategoryByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdAsync(query.Id, cancellationToken);
        if (category is null || category.UserId != _currentUser.UserId)
            throw new NotFoundException("Category not found.");

        return CategoryResponse.From(category);
    }
}
