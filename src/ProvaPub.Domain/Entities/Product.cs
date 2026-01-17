using ProvaPub.Domain.Entities.Base;

namespace ProvaPub.Domain.Entities;

public sealed class Product : EntityBase
{
    public string Name { get; private set; } = string.Empty;

    private Product()
    {
    }

    private Product(string name)
    {
        Name = name;
    }

    public static Product Create(string name)
        => new(name);
}