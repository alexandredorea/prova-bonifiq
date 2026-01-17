using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProvaPub.Application.Commons.Models;
using ProvaPub.Application.Features.RandomNumbers.Commands;

namespace ProvaPub.Api.Controllers;

/// <summary>
/// Resolução da parte 1: Geração de números aleatórios dentro de um range
/// </summary>
/// <param name="sender"></param>
[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public sealed class NumbersController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateRandomNumberCommand(), cancellationToken);

        if (!result.Success)
            return Conflict(result);

        return Ok(result);
    }
}