using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProvaPub.Application.Commons.Models;
using ProvaPub.Application.DTOs;
using ProvaPub.Application.Features.Products.Queries;

namespace ProvaPub.Api.Controllers;

/// <summary>
/// Resolução da parte 2: retorno de uma lista páginada de produtos.
/// </summary>
/// <param name="sender"></param>
[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PagedResult<ProductDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListProducts(int page, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductsQuery(page), cancellationToken);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}