using MediatR;
using ProvaPub.Application.Commons.Cqs;
using ProvaPub.Application.Commons.Models;
using ProvaPub.Application.Commons.Patterns;
using ProvaPub.Application.Commons.Persistences;
using ProvaPub.Application.DTOs;
using ProvaPub.Domain.Entities;

namespace ProvaPub.Application.Features.Orders.Commands;

public sealed record ProcessPaymentCommand(
    string PaymentMethod,
    decimal PaymentValue,
    int CustomerId) : ICommand<Result<OrderDto>>;

internal sealed class ProcessPaymentCommandHandler(
    IProvaPubContext context,
    IPaymentFactory paymentFactory) : IRequestHandler<ProcessPaymentCommand, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var strategy = paymentFactory.Resolve(request.PaymentMethod);
        await strategy.ProcessAsync(request.PaymentValue, request.CustomerId);

        var order = Order.Create(request.PaymentValue, request.CustomerId);
        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);

        return Result<OrderDto>.Ok(order!);
    }
}