using FinTrack.Domain.Categories;

namespace FinTrack.Application.Categories;

public sealed record CategoryResponse(Guid Id, string Name, string Type, string Color)
{
    public static CategoryResponse From(Category category) => new(
        category.Id,
        category.Name,
        category.Type.ToString(),
        category.Color);
}
