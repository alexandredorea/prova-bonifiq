using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProvaPub.Application.Commons.Models;
using ProvaPub.Application.DTOs;
using ProvaPub.Application.Features.Customers.Commands;
using ProvaPub.Application.Features.Customers.Queries;

namespace ProvaPub.Api.Controllers;

/// <summary>
///
/// </summary>
/// <param name="sender"></param>
[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public sealed class CustomersController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Resolução da parte 2: retorno de uma lista páginada de clientes.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResult<CustomerDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PagedResult<CustomerDto>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListProducts(int page, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCustomersQuery(page), cancellationToken);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Resolução da parte 4.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("{id:int}/purchase")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CanPurchase(
        [FromRoute] int id,
        [FromBody] ValidatePurchaseCommand command,
        CancellationToken cancellationToken)
    {
        command.SetCustomerId(id);
        var result = await sender.Send(command, cancellationToken);
        return Ok(result);
    }
}