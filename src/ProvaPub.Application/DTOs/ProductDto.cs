using ProvaPub.Domain.Entities;

namespace ProvaPub.Application.DTOs;

public sealed record ProductDto(int Id, string Name)
{
    public static implicit operator ProductDto?(Product? product)
    {
        if (product is null)
            return null;

        return new(product.Id, product.Name);
    }
}