namespace ProvaPub.Models
{
    public sealed class PagedResult<T>
    {
        public int TotalCount { get; init; }
        public bool HasNext { get; init; }
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    }
}