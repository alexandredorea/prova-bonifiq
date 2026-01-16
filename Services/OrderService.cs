using ProvaPub.Models;
using ProvaPub.Repository;

namespace ProvaPub.Services
{
    public interface IOrderService
    {
        Task<Order> InsertOrder(Order order, CancellationToken cancellationToken = default);

        Task<Order> PayOrder(string paymentMethod, decimal paymentValue, int customerId, CancellationToken cancellationToken = default);
    }

    public sealed class OrderService : IOrderService
    {
        private readonly TestDbContext _ctx;
        private readonly IPaymentFactory _paymentFactory;
        private readonly TimeZoneInfo _tz;

        public OrderService(
            TestDbContext ctx,
            IPaymentFactory paymentFactory,
            TimeZoneInfo tz)
        {
            _ctx = ctx;
            _paymentFactory = paymentFactory;
            _tz = tz;
        }

        public async Task<Order> PayOrder(string paymentMethod, decimal paymentValue, int customerId, CancellationToken cancellationToken = default)
        {
            var strategy = _paymentFactory.Resolve(paymentMethod);
            await strategy.ProcessAsync(paymentValue, customerId);
            var order = new Order()
            {
                Value = paymentValue,
                CustomerId = customerId,
                OrderDate = DateTime.UtcNow
            };
            await InsertOrder(order, cancellationToken);

            order.OrderDate = TimeZoneInfo.ConvertTimeFromUtc(order.OrderDate, _tz);
            return order;
        }

        public async Task<Order> InsertOrder(Order order, CancellationToken cancellationToken = default)
        {
            //Insere pedido no banco de dados
            _ctx.Orders.Add(order);
            await _ctx.SaveChangesAsync(cancellationToken);
            return order;
        }
    }
}