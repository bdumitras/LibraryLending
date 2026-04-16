using LibraryLending.Application.Features.Books.Contracts;
using LibraryLending.Domain.Entities;

namespace LibraryLending.Application.Features.Books.Mappings;

public static class BookMappings
{
    public static BookResponse ToResponse(this Book book)
    {
        ArgumentNullException.ThrowIfNull(book);

        return new BookResponse
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Isbn = book.Isbn,
            PublicationYear = book.PublicationYear,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies,
            HasAvailableCopies = book.HasAvailableCopies,
            CreatedAtUtc = book.CreatedAtUtc,
            UpdatedAtUtc = book.UpdatedAtUtc
        };
    }
}
