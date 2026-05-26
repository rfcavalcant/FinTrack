using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Categories;
using MediatR;

namespace FinTrack.Application.Categories;

public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryResponse>>;

public sealed class GetCategoriesQueryHandler
    : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryResponse>>
{
    private readonly ICategoryRepository _categories;
    private readonly ICurrentUserService _currentUser;

    public GetCategoriesQueryHandler(ICategoryRepository categories, ICurrentUserService currentUser)
    {
        _categories = categories;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<CategoryResponse>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _categories.GetByUserAsync(_currentUser.UserId, cancellationToken);
        return categories.Select(CategoryResponse.From).ToList();
    }
}
