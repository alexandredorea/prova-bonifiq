using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProvaPub.Application.Commons.Persistences;
using ProvaPub.Application.Features.Orders.Commands;

namespace ProvaPub.Application.Features.Orders.Validations;

public sealed class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    private readonly IProvaPubContext _context;

    public ProcessPaymentCommandValidator(IProvaPubContext context)
    {
        _context = context;

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("É necessário fornecer um método de pagamento.")
            .Must(BeValidPaymentMethod)
            .WithMessage("O método de pagamento deve ser 'pix', 'creditcard' ou 'paypal'.");

        RuleFor(x => x.PaymentValue)
            .GreaterThan(0)
            .WithMessage("O valor do pagamento deve ser maior que zero.")
            .LessThanOrEqualTo(1_000_000)
            .WithMessage("O valor do pagamento não pode exceder 1.000.000.");

        When(x => x.CustomerId > 0, () =>
        {
            RuleFor(x => x.CustomerId)
                .MustAsync(BeAnExistingCustomer)
                .WithMessage($"Cliente não encontrado.");

        }).Otherwise(() => 
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0)
                .WithMessage("O ID do cliente deve ser maior que zero.");
        });
        
    }

    private async Task<bool> BeAnExistingCustomer(int customerId, CancellationToken token)
    {
        return await _context.Customers.AnyAsync(c => c.Id == customerId, token);
    }

    private static bool BeValidPaymentMethod(string method)
    {
        var validMethods = new[] { "pix", "creditcard", "paypal" };
        return validMethods.Contains(method?.ToLowerInvariant());
    }
}