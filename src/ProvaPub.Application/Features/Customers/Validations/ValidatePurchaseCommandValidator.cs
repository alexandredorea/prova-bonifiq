using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProvaPub.Application.Commons.Middlewares;
using ProvaPub.Application.Commons.Persistences;
using ProvaPub.Application.Features.Customers.Commands;

namespace ProvaPub.Application.Features.Customers.Validations;

public sealed class ValidatePurchaseCommandValidator : AbstractValidator<ValidatePurchaseCommand>
{
    private readonly IProvaPubContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ValidatePurchaseCommandValidator(
        IProvaPubContext context,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;

        RuleFor(x => x.PurchaseValue)
            .GreaterThan(0)
            .WithMessage("O valor da compra deve ser maior que zero.");

        When(x => x.CustomerId > 0, () =>
        {
            RuleFor(x => x.CustomerId)
                .MustAsync(CustomerExists)
                .WithMessage(x => $"O ID do cliente {x.CustomerId} não encontrado.");

            RuleFor(x => x)
                .MustAsync(OnlyOnePurchasePerMonth)
                .WithName("CustomerId")
                .WithMessage("Um cliente pode efetuar apenas uma compra por mês.");

            RuleFor(x => x)
                .MustAsync(FirstPurchaseMaxValue)
                .WithName("CustomerId")
                .WithMessage("Um cliente que nunca comprou antes pode fazer uma primeira compra de no máximo 100,00.");
        }).Otherwise(() =>
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0)
                .WithMessage("O ID do cliente deve ser maior que zero.");
        });

        RuleFor(x => x)
            .Must(DuringBusinessHours)
            .WithName("CustomerId")
            .WithMessage("O cliente só pode efetuar compras durante o horário comercial (das 8h às 18h) e em dias úteis (de segunda a sexta-feira).");
    }

    // Business Rule: Non registered Customers cannot purchase
    private async Task<bool> CustomerExists(int customerId, CancellationToken cancellationToken)
    {
        return await _context.Customers
            .AnyAsync(c => c.Id == customerId, cancellationToken);
    }

    // Business Rule: A customer can purchase only a single time per month
    private async Task<bool> OnlyOnePurchasePerMonth(
        ValidatePurchaseCommand command,
        CancellationToken cancellationToken)
    {
        var baseDate = _dateTimeProvider.UtcNow.AddMonths(-1);

        var ordersInThisMonth = await _context.Orders
            .CountAsync(o => o.CustomerId == command.CustomerId && o.OrderDate >= baseDate, cancellationToken);

        return ordersInThisMonth == 0;
    }

    // Business Rule: A customer that never bought before can make a first purchase of maximum 100,00
    private async Task<bool> FirstPurchaseMaxValue(
        ValidatePurchaseCommand command,
        CancellationToken cancellationToken)
    {
        var haveBoughtBefore = await _context.Orders
            .AnyAsync(o => o.CustomerId == command.CustomerId, cancellationToken);

        if (!haveBoughtBefore && command.PurchaseValue > 100)
            return false;

        return true;
    }

    // Business Rule: A customer can purchases only during business hours and working days
    private bool DuringBusinessHours(ValidatePurchaseCommand command)
    {
        var now = _dateTimeProvider.LocalNow;

        var isOutsideBusinessHours = now.Hour < 8 || now.Hour > 18;
        var isWeekend = now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday;

        return !isOutsideBusinessHours && !isWeekend;
    }
}