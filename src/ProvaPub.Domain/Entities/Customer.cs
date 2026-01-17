using ProvaPub.Domain.Entities.Base;

namespace ProvaPub.Domain.Entities;

public sealed class Customer : EntityBase
{
    public string Name { get; private set; } = string.Empty;

    private readonly List<Order> _orders = [];
    public IReadOnlyCollection<Order?> Orders => _orders.AsReadOnly();

    private Customer()
    {
    }

    private Customer(string name)
    {
        Name = name;
    }

    public static Customer Create(string name)
        => new(name);

    public void AddOrder(Order order)
    {
        _orders.Add(order);
    }
}