using Microsoft.EntityFrameworkCore;

namespace ProvaPub.Application.Commons.Models;

public sealed record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
    public bool IsFirstPage => Page == 1;
    public bool IsLastPage => Page >= TotalPages;
}

public static class QueryableExtensions
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;

    public static async Task<PagedResult<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> query,
        int page = 1,
        int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = DefaultPageSize;

        if (pageSize > MaxPageSize)
            pageSize = MaxPageSize;

        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return new PagedResult<T>
            {
                Items = Array.Empty<T>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = 0
            };
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}

public static class PagedResultExtensions
{
    public static PagedResult<TDestination> Map<TSource, TDestination>(
        this PagedResult<TSource> source,
        Func<TSource, TDestination> mapper)
    {
        return new PagedResult<TDestination>
        {
            Items = source.Items?.Select(mapper).ToList()!,
            Page = source.Page,
            PageSize = source.PageSize,
            TotalCount = source.TotalCount
        };
    }
}