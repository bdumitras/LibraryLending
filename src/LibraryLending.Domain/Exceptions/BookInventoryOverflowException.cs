namespace LibraryLending.Domain.Exceptions;

public sealed class BookInventoryOverflowException : DomainException
{
    public BookInventoryOverflowException(Guid bookId, string isbn)
        : base($"Book '{isbn}' ({bookId}) cannot accept a returned copy because all copies are already available.")
    {
        BookId = bookId;
        Isbn = isbn;
    }

    public Guid BookId { get; }

    public string Isbn { get; }
}
