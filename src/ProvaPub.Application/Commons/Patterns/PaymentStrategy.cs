using Microsoft.Extensions.Logging;

namespace ProvaPub.Application.Commons.Patterns;

public interface IPaymentStrategy
{
    string Method { get; }

    Task ProcessAsync(decimal amount, int customerId);
}

public sealed class PixPaymentStrategy : IPaymentStrategy
{
    private readonly ILogger<PixPaymentStrategy> _logger;

    public PixPaymentStrategy(ILogger<PixPaymentStrategy> logger)
    {
        _logger = logger;
    }

    public string Method => "pix";

    public Task ProcessAsync(decimal amount, int customerId)
    {
        // lógica PIX
        _logger.LogInformation("[Pix] : Pagamento realizado com sucesso");
        return Task.CompletedTask;
    }
}

public sealed class CreditCardPaymentStrategy : IPaymentStrategy
{
    private readonly ILogger<CreditCardPaymentStrategy> _logger;

    public CreditCardPaymentStrategy(ILogger<CreditCardPaymentStrategy> logger)
    {
        _logger = logger;
    }

    public string Method => "creditcard";

    public Task ProcessAsync(decimal amount, int customerId)
    {
        // lógica cartão
        _logger.LogInformation("[Cartão de crédito] : Pagamento realizado com sucesso");
        return Task.CompletedTask;
    }
}

public sealed class PaypalPaymentStrategy : IPaymentStrategy
{
    private readonly ILogger<PaypalPaymentStrategy> _logger;

    public PaypalPaymentStrategy(ILogger<PaypalPaymentStrategy> logger)
    {
        _logger = logger;
    }

    public string Method => "paypal";

    public Task ProcessAsync(decimal amount, int customerId)
    {
        // lógica PayPal
        _logger.LogInformation("[PayPal] : Pagamento realizado com sucesso");
        return Task.CompletedTask;
    }
}