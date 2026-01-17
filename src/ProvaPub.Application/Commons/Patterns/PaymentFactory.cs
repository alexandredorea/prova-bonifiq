namespace ProvaPub.Application.Commons.Patterns;

public interface IPaymentFactory
{
    IPaymentStrategy Resolve(string method);
}

public sealed class PaymentFactory : IPaymentFactory
{
    private readonly IEnumerable<IPaymentStrategy> _strategies;

    public PaymentFactory(IEnumerable<IPaymentStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IPaymentStrategy Resolve(string method)
    {
        var strategy = _strategies.FirstOrDefault(x => x.Method.Equals(method, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotSupportedException($"Método de pagamento '{method}' não suportado.");

        return strategy;
    }
}