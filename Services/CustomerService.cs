using Microsoft.EntityFrameworkCore;
using ProvaPub.Models;
using ProvaPub.Repository;

namespace ProvaPub.Services
{
    public interface ICustomerService
    {
        Task<bool> CanPurchase(int customerId, decimal purchaseValue);

        Task<PagedResult<Customer>> ListCustomers(int page, CancellationToken cancellationToken = default);
    }

    public sealed class CustomerService : ICustomerService
    {
        private readonly TestDbContext _ctx;
        private readonly IPaginatorService _paginator;

        public CustomerService(TestDbContext ctx, IPaginatorService paginator)
        {
            _ctx = ctx;
            _paginator = paginator;
        }

        public async Task<PagedResult<Customer>> ListCustomers(int page, CancellationToken cancellationToken = default)
        {
            return await _paginator.PaginateAsync(
                query: _ctx.Customers.AsNoTracking(),
                page: page,
                pageSize: 10,
                cancellationToken: cancellationToken);
        }

        public async Task<bool> CanPurchase(int customerId, decimal purchaseValue)
        {
            if (customerId <= 0)
                throw new ArgumentOutOfRangeException(nameof(customerId));

            if (purchaseValue <= 0)
                throw new ArgumentOutOfRangeException(nameof(purchaseValue));

            //Business Rule: Non registered Customers cannot purchase
            var customer = await _ctx.Customers.FindAsync(customerId)
                ?? throw new InvalidOperationException($"Customer Id {customerId} does not exists");

            //Business Rule: A customer can purchase only a single time per month
            var baseDate = DateTime.UtcNow.AddMonths(-1);
            var ordersInThisMonth = await _ctx.Orders.CountAsync(s => s.CustomerId == customerId && s.OrderDate >= baseDate);
            if (ordersInThisMonth > 0)
                return false;

            //Business Rule: A customer that never bought before can make a first purchase of maximum 100,00
            var haveBoughtBefore = await _ctx.Customers.CountAsync(s => s.Id == customerId && s.Orders.Any());
            if (haveBoughtBefore == 0 && purchaseValue > 100)
                return false;

            //Business Rule: A customer can purchases only during business hours and working days
            if (DateTime.UtcNow.Hour < 8 || DateTime.UtcNow.Hour > 18 || DateTime.UtcNow.DayOfWeek == DayOfWeek.Saturday || DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday)
                return false;

            return true;
        }
    }
}