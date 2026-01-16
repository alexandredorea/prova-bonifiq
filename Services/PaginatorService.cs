using Microsoft.EntityFrameworkCore;
using ProvaPub.Models;

namespace ProvaPub.Services
{
    public interface IPaginatorService
    {
        Task<PagedResult<T>> PaginateAsync<T>(
            IQueryable<T> query,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
    }

    public sealed class PaginatorService : IPaginatorService
    {
        public async Task<PagedResult<T>> PaginateAsync<T>(
            IQueryable<T> query,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<T>
            {
                TotalCount = total,
                HasNext = (page * pageSize) < total,
                Items = items
            };
        }
    }
}