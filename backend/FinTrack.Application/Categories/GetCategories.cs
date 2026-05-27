using FinTrack.Application.Common;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Categories;

namespace FinTrack.Application.Categories;

public sealed record GetCategoriesQuery;

public sealed class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, IReadOnlyList<CategoryResponse>>
{
    private readonly ICategoryRepository _categories;
    private readonly ICurrentUserService _currentUser;

    public GetCategoriesQueryHandler(ICategoryRepository categories, ICurrentUserService currentUser)
    {
        _categories = categories;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<CategoryResponse>> HandleAsync(
        GetCategoriesQuery query,
        CancellationToken cancellationToken = default)
    {
        var categories = await _categories.GetByUserAsync(_currentUser.UserId, cancellationToken);
        return categories.Select(CategoryResponse.From).ToList();
    }
}
