namespace LibraryLending.Application.Common.Models;

public record PagedResponse<T>
{
    public required IReadOnlyCollection<T> Items { get; init; }

    public required int Page { get; init; }

    public required int PageSize { get; init; }

    public required int TotalCount { get; init; }

    public int TotalPages => TotalCount <= 0
        ? 0
        : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => TotalPages > 0 && Page < TotalPages;
}
