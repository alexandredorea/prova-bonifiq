using System.Reflection;
using Bogus;
using Microsoft.EntityFrameworkCore;
using ProvaPub.Domain.Entities;

namespace ProvaPub.Infrastructure.Persistences.Seed;

public static class DatabaseSeeder
{
    public static void SeedCustomers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasData(GetCustomerSeed());
    }

    public static void SeedProducts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(GetProductSeed());
    }

    private static Customer[] GetCustomerSeed()
    {
        // Reflection para criar instâncias com construtor privado
        var customerType = typeof(Customer);
        List<Customer> result = [];

        for (int i = 1; i <= 20; i++)
        {
            var customer = Activator.CreateInstance(customerType, nonPublic: true);
            SetProperty(customer, $"{nameof(Customer.Id)}", i);
            SetProperty(customer, $"{nameof(Customer.Name)}", new Faker().Person.FullName);
            result.Add((Customer)customer!);
        }

        return result.ToArray();
    }

    private static Product[] GetProductSeed()
    {
        var productType = typeof(Product);
        List<Product> result = [];

        for (int i = 1; i <= 20; i++)
        {
            var product = Activator.CreateInstance(productType, nonPublic: true);
            SetProperty(product, $"{nameof(Product.Id)}", i);
            SetProperty(product, $"{nameof(Product.Name)}", new Faker().Commerce.ProductName());
            result.Add((Product)product!);
        }

        return result.ToArray();
    }

    /// <summary>
    /// Define propriedades via reflection
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    private static void SetProperty(object? obj, string propertyName, object value)
    {
        if (obj == null) return;

        var property = obj.GetType().GetProperty(propertyName,
            BindingFlags.Public | BindingFlags.Instance);

        property?.SetValue(obj, value);
    }
}