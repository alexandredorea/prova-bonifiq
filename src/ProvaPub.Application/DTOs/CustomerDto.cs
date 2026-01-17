using ProvaPub.Domain.Entities;

namespace ProvaPub.Application.DTOs;

public sealed record CustomerDto(
    int Id,
    string Name,
    IReadOnlyCollection<OrderDto> Orders)
{
    public static implicit operator CustomerDto?(Customer? customer)
    {
        if (customer is null)
            return null;

        return new CustomerDto(
            customer.Id,
            customer.Name,
            customer.Orders.Select(o => (OrderDto?)o).ToList()!);
    }
}