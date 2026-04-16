using LibraryLending.Application.Common.Models;

namespace LibraryLending.Application.Features.Books.Models;

public sealed record BookListFilter : PagedRequest
{
    public string? SearchTerm { get; init; }

    public string? Title { get; init; }

    public string? Author { get; init; }

    public string? Isbn { get; init; }

    public int? PublicationYearFrom { get; init; }

    public int? PublicationYearTo { get; init; }

    public bool? IsAvailable { get; init; }

    public string? SortBy { get; init; }

    public SortDirection SortDirection { get; init; } = SortDirection.Asc;
}
