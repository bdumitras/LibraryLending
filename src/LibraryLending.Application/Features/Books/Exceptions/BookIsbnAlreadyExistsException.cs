using LibraryLending.Application.Common.Exceptions;

namespace LibraryLending.Application.Features.Books.Exceptions;

public sealed class BookIsbnAlreadyExistsException : ConflictException
{
    public BookIsbnAlreadyExistsException(string isbn)
        : base($"A book with ISBN '{isbn}' already exists.")
    {
    }
}
