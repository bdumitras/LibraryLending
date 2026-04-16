namespace LibraryLending.Domain.Exceptions;

public sealed class BookHasNoAvailableCopiesException : DomainException
{
    public BookHasNoAvailableCopiesException(Guid bookId, string isbn)
        : base($"Book '{isbn}' ({bookId}) has no available copies to loan.")
    {
        BookId = bookId;
        Isbn = isbn;
    }

    public Guid BookId { get; }

    public string Isbn { get; }
}
