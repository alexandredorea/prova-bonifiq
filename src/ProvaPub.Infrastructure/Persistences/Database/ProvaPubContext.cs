using Microsoft.EntityFrameworkCore;
using ProvaPub.Application.Commons.Persistences;
using ProvaPub.Domain.Entities;
using ProvaPub.Infrastructure.Persistences.Seed;

namespace ProvaPub.Infrastructure.Persistences.Database;

public sealed class ProvaPubContext(DbContextOptions<ProvaPubContext> options) : DbContext(options), IProvaPubContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<RandomNumber> Numbers => Set<RandomNumber>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProvaPubContext).Assembly);

        DatabaseSeeder.SeedCustomers(modelBuilder);
        DatabaseSeeder.SeedProducts(modelBuilder);
    }
}