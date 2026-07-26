namespace Shared.Kernel.Pagination;

public sealed record PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }

    public required PageInfo Page { get; init; }

    public static PagedResult<T> Create(IReadOnlyList<T> items, int totalItems, PageRequest request) =>
        new()
        {
            Items = items,
            Page = new PageInfo
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalItems = totalItems,
                TotalPages = totalItems == 0
                    ? 0
                    : (int)Math.Ceiling(totalItems / (double)request.PageSize)
            }
        };

    public static PagedResult<T> Empty(PageRequest request) =>
        Create(Array.Empty<T>(), 0, request);
}
