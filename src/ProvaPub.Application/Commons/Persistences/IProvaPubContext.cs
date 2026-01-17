using Microsoft.EntityFrameworkCore;
using ProvaPub.Domain.Entities;

namespace ProvaPub.Application.Commons.Persistences;

public interface IProvaPubContext
{
    DbSet<Customer> Customers { get; }
    DbSet<Order> Orders { get; }
    DbSet<Product> Products { get; }
    DbSet<RandomNumber> Numbers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}