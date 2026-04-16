using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Books.Models;

namespace LibraryLending.Api.Models.Books;

public sealed class GetBooksRequestQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? SearchTerm { get; set; }

    public string? Title { get; set; }

    public string? Author { get; set; }

    public string? Isbn { get; set; }

    public int? PublicationYearFrom { get; set; }

    public int? PublicationYearTo { get; set; }

    public bool? IsAvailable { get; set; }

    public string? SortBy { get; set; }

    public SortDirection SortDirection { get; set; } = SortDirection.Asc;

    public BookListFilter ToFilter()
    {
        return new BookListFilter
        {
            Page = Page,
            PageSize = PageSize,
            SearchTerm = SearchTerm,
            Title = Title,
            Author = Author,
            Isbn = Isbn,
            PublicationYearFrom = PublicationYearFrom,
            PublicationYearTo = PublicationYearTo,
            IsAvailable = IsAvailable,
            SortBy = SortBy,
            SortDirection = SortDirection
        };
    }
}
