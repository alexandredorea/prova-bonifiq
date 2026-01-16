namespace ProvaPub.Services
{
    public interface IPaymentStrategy
    {
        string Method { get; }

        Task ProcessAsync(decimal amount, int customerId);
    }

    public sealed class PixPaymentStrategy : IPaymentStrategy
    {
        public string Method => "pix";

        public Task ProcessAsync(decimal amount, int customerId)
        {
            // lógica PIX
            return Task.CompletedTask;
        }
    }

    public sealed class CreditCardPaymentStrategy : IPaymentStrategy
    {
        public string Method => "creditcard";

        public Task ProcessAsync(decimal amount, int customerId)
        {
            // lógica cartão
            return Task.CompletedTask;
        }
    }

    public sealed class PaypalPaymentStrategy : IPaymentStrategy
    {
        public string Method => "paypal";

        public Task ProcessAsync(decimal amount, int customerId)
        {
            // lógica PayPal
            return Task.CompletedTask;
        }
    }
}