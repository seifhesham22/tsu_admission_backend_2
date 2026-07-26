namespace Shared.Kernel.Pagination;

public sealed record PageInfo
{
    public required int PageNumber { get; init; }

    public required int PageSize { get; init; }

    public required int TotalItems { get; init; }

    public required int TotalPages { get; init; }

    public bool HasPrevious => PageNumber > 1;

    public bool HasNext => PageNumber < TotalPages;
}
