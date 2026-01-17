using System.Text.Json.Serialization;
using MediatR;
using Microsoft.Extensions.Logging;
using ProvaPub.Application.Commons.Models;

namespace ProvaPub.Application.Features.Customers.Commands;

public sealed class ValidatePurchaseCommand : IRequest<Result<bool>>
{
    public ValidatePurchaseCommand()
    {
    }

    public ValidatePurchaseCommand(decimal purchaseValue)
    {
        PurchaseValue = purchaseValue;
    }

    [JsonIgnore]
    public int CustomerId { get; private set; }

    public decimal PurchaseValue { get; init; }

    public void SetCustomerId(int customerId)
    {
        CustomerId = customerId;
    }
}

internal sealed class ValidatePurchaseCommandHandler(ILogger<ValidatePurchaseCommandHandler> logger)
    : IRequestHandler<ValidatePurchaseCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(ValidatePurchaseCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Compra do cliente {CustomerId} com o valor {PurchaseValue:C}. Efetuada com sucesso.",
            request.CustomerId,
            request.PurchaseValue);

        // A validação acontece automaticamente no ValidationBehavior
        // Se chegou aqui, todas as regras passaram
        return Task.FromResult(Result<bool>.Ok(true));
    }
}