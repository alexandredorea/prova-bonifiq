using MediatR;
using Microsoft.EntityFrameworkCore;
using ProvaPub.Application.Commons.Cqs;
using ProvaPub.Application.Commons.Models;
using ProvaPub.Application.Commons.Persistences;
using ProvaPub.Application.DTOs;

namespace ProvaPub.Application.Features.Customers.Queries;

public sealed record GetCustomersQuery(int Page, int PageSize = 10)
    : IQuery<Result<PagedResult<CustomerDto>>>;

internal sealed class GetCustomersQueryHandler(IProvaPubContext context) : IRequestHandler<GetCustomersQuery, Result<PagedResult<CustomerDto>>>
{
    public async Task<Result<PagedResult<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var result = await context.Customers
            .Include(c => c.Orders)
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Select(c => new CustomerDto(
                c.Id,
                c.Name,
                c.Orders.Select(o => new OrderDto(
                    o!.Id,
                    o.Value,
                    o.CustomerId,
                    o.OrderDate)).ToList())
            ).ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken)
            //.ContinueWith(t => t.Result.Map(p => (ProductDto?)p), cancellationToken)
            ;

        if (!result.Items.Any())
            return Result<PagedResult<CustomerDto>>.NotFound("Nenhum cliente cadastrado.");

        return Result<PagedResult<CustomerDto>>.Ok(result!);
    }
}