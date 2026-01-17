using MediatR;
using Microsoft.EntityFrameworkCore;
using ProvaPub.Application.Commons.Cqs;
using ProvaPub.Application.Commons.Models;
using ProvaPub.Application.Commons.Persistences;
using ProvaPub.Application.DTOs;

namespace ProvaPub.Application.Features.Products.Queries;

public sealed record GetProductsQuery(int Page, int PageSize = 10)
    : IQuery<Result<PagedResult<ProductDto>>>;

internal sealed class GetProductsQueryHandler(IProvaPubContext context) : IRequestHandler<GetProductsQuery, Result<PagedResult<ProductDto>>>
{
    public async Task<Result<PagedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var result = await context.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new ProductDto(p.Id, p.Name))
            .ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken)
            //.ContinueWith(t => t.Result.Map(p => (ProductDto?)p), cancellationToken)
            ;

        if (!result.Items.Any())
            return Result<PagedResult<ProductDto>>.NotFound("Nenhum produto cadastrado.");

        return Result<PagedResult<ProductDto>>.Ok(result!);
    }
}