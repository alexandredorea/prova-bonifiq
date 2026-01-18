using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProvaPub.Application.Commons.Models;
using ProvaPub.Application.DTOs;
using ProvaPub.Application.Features.Orders.Commands;

namespace ProvaPub.Api.Controllers;

/// <summary>
/// Resolução da parte 3: realizar pagamentos por diversos métodos um pouco mais adequada.
/// </summary>
/// <param name="sender"></param>
[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public sealed class OrdersController(ISender sender) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ApiExplorerSettings(IgnoreApi = true)] //escondendo o path, pois nao faz parte do teste
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
    public IActionResult GetOrderById(int id, CancellationToken cancellationToken)
    {
        return Ok(Result<int>.Ok(id));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<OrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<OrderDto>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(Result<OrderDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] ProcessPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request, cancellationToken);
        return CreatedAtAction(
            actionName: nameof(GetOrderById),
            routeValues: new { id = result?.Data?.Id },
            value: result);
    }
}