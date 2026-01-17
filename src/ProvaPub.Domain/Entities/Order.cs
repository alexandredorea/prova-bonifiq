using ProvaPub.Domain.Entities.Base;

namespace ProvaPub.Domain.Entities;

public sealed class Order : EntityBase
{
    public decimal Value { get; private set; }
    public DateTime OrderDate { get; init; }
    public int? CustomerId { get; private set; }
    public Customer? Customer { get; init; } = default!;

    private Order()
    {
    }

    private Order(decimal value, int customerId)
    {
        Value = value;
        CustomerId = customerId;
    }

    public static Order Create(decimal value, int customerId)
        => new(value, customerId);
}