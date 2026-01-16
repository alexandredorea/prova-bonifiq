using Microsoft.EntityFrameworkCore;
using ProvaPub.Models;
using ProvaPub.Repository;

namespace ProvaPub.Services
{
    public interface IProductService
    {
        Task<PagedResult<Product>> ListProducts(int page, CancellationToken cancellationToken = default);
    }

    public sealed class ProductService : IProductService
    {
        private readonly TestDbContext _ctx;
        private readonly IPaginatorService _paginator;

        public ProductService(TestDbContext ctx, IPaginatorService paginator)
        {
            _ctx = ctx;
            _paginator = paginator;
        }

        public async Task<PagedResult<Product>> ListProducts(int page, CancellationToken cancellationToken = default)
        {
            return await _paginator.PaginateAsync(
                query: _ctx.Products.AsNoTracking(),
                page: page,
                pageSize: 10,
                cancellationToken: cancellationToken);
        }
    }
}