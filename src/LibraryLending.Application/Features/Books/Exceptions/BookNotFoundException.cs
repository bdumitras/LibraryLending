using LibraryLending.Application.Common.Exceptions;

namespace LibraryLending.Application.Features.Books.Exceptions;

public sealed class BookNotFoundException : NotFoundException
{
    public BookNotFoundException(Guid bookId)
        : base($"Book '{bookId}' was not found.")
    {
    }
}
