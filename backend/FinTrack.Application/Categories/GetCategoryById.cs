using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Categories;
using MediatR;

namespace FinTrack.Application.Categories;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryResponse>;

public sealed class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryResponse>
{
    private readonly ICategoryRepository _categories;
    private readonly ICurrentUserService _currentUser;

    public GetCategoryByIdQueryHandler(ICategoryRepository categories, ICurrentUserService currentUser)
    {
        _categories = categories;
        _currentUser = currentUser;
    }

    public async Task<CategoryResponse> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdAsync(request.Id, cancellationToken);
        if (category is null || category.UserId != _currentUser.UserId)
        {
            throw new NotFoundException("Category not found.");
        }

        return CategoryResponse.From(category);
    }
}
