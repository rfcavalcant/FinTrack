using FinTrack.Domain.Common;

namespace FinTrack.Domain.Categories;

// Raiz de agregado. Classifica lançamentos. Pertence a um User (por Id).
public sealed class Category : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public CategoryType Type { get; private set; }
    public string Color { get; private set; } = string.Empty;

    private Category()
    {
    }

    private Category(Guid id, Guid userId, string name, CategoryType type, string color) : base(id)
    {
        UserId = userId;
        Name = name;
        Type = type;
        Color = color;
    }

    public static Category Create(Guid userId, string name, CategoryType type, string color)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("Category must belong to a user.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category name is required.");
        }

        return new Category(Guid.NewGuid(), userId, name.Trim(), type, NormalizeColor(color));
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category name is required.");
        }

        Name = name.Trim();
    }

    public void ChangeColor(string color) => Color = NormalizeColor(color);

    private static string NormalizeColor(string color)
        => string.IsNullOrWhiteSpace(color) ? string.Empty : color.Trim();
}
