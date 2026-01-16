using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProvaPub.Models;
using ProvaPub.Repository;
using ProvaPub.Services;
using Xunit;

namespace ProvaPub.UnitTests.Services
{
    public sealed class CustomerServiceTest
    {
        private static CustomerService CreateService(
            Action<TestDbContext>? seed = null)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var ctx = new TestDbContext(options);

            seed?.Invoke(ctx);
            ctx.SaveChanges();

            var paginator = Mock.Of<IPaginatorService>();

            return new CustomerService(ctx, paginator);
        }

        [Fact]
        public async Task CanPurchase_WhenCustomerIdIsZero_Throws()
        {
            var service = CreateService();

            Func<Task> act = () => service.CanPurchase(0, 100);

            await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async Task CanPurchase_WhenCustomerDoesNotExist_Throws()
        {
            var service = CreateService();

            Func<Task> act = () => service.CanPurchase(1, 50);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task CanPurchase_WhenCustomerBoughtThisMonth_ReturnsFalse()
        {
            var now = new DateTime(2024, 10, 15, 10, 0, 0, DateTimeKind.Local);

            var service = CreateService(ctx =>
            {
                ctx.Customers.Add(new Customer { Id = 1, Name = "Teste" });
                ctx.Orders.Add(new Order { CustomerId = 1, OrderDate = now.AddDays(-5) });
            });

            var result = await service.CanPurchase(1, 50);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanPurchase_WhenFirstPurchaseOver100_ReturnsFalse()
        {
            var service = CreateService(ctx =>
            {
                ctx.Customers.Add(new Customer { Id = 1, Name = "Teste" });
            });

            var result = await service.CanPurchase(1, 150);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanPurchase_OutsideBusinessHours_ReturnsFalse()
        {
            var service = CreateService(ctx =>
            {
                ctx.Customers.Add(new Customer { Id = 1, Name = "Teste" });
            });

            var result = await service.CanPurchase(1, 50);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanPurchase_HappyPath_ReturnsTrue()
        {
            //var now = new DateTime(2024, 10, 15, 10, 0, 0, DateTimeKind.Utc); // Quarta 10h

            var service = CreateService(ctx =>
            {
                ctx.Customers.Add(new Customer { Id = 1, Name = "Teste" });
            });

            var result = await service.CanPurchase(1, 50);

            result.Should().BeTrue();
        }
    }
}