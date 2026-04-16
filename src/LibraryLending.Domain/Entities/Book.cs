namespace LibraryLending.Domain.Entities;

using LibraryLending.Domain.Common;
using LibraryLending.Domain.Exceptions;

public class Book : AuditableEntity
{
    private static readonly int MaxPublicationYear = DateTime.UtcNow.Year + 1;

    private Book()
    {
    }

    public Book(
        string title,
        string author,
        string isbn,
        int publicationYear,
        int totalCopies,
        int availableCopies)
    {
        ApplyDetails(title, author, isbn, publicationYear, totalCopies, availableCopies, touch: false);
    }

    public string Title { get; private set; } = string.Empty;

    public string Author { get; private set; } = string.Empty;

    public string Isbn { get; private set; } = string.Empty;

    public int PublicationYear { get; private set; }

    public int TotalCopies { get; private set; }

    public int AvailableCopies { get; private set; }

    public bool HasAvailableCopies => AvailableCopies > 0;

    public void UpdateDetails(
        string title,
        string author,
        string isbn,
        int publicationYear,
        int totalCopies,
        int availableCopies)
    {
        ApplyDetails(title, author, isbn, publicationYear, totalCopies, availableCopies, touch: true);
    }

    public void BorrowCopy()
    {
        if (!HasAvailableCopies)
        {
            throw new BookHasNoAvailableCopiesException(Id, Isbn);
        }

        AvailableCopies -= 1;
        Touch();
    }

    public void ReturnCopy()
    {
        if (AvailableCopies >= TotalCopies)
        {
            throw new BookInventoryOverflowException(Id, Isbn);
        }

        AvailableCopies += 1;
        Touch();
    }

    private void ApplyDetails(
        string title,
        string author,
        string isbn,
        int publicationYear,
        int totalCopies,
        int availableCopies,
        bool touch)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainValidationException("Book title is required.");
        }

        if (string.IsNullOrWhiteSpace(author))
        {
            throw new DomainValidationException("Book author is required.");
        }

        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new DomainValidationException("Book ISBN is required.");
        }

        if (publicationYear < 1450 || publicationYear > MaxPublicationYear)
        {
            throw new DomainValidationException($"Book publication year '{publicationYear}' is outside the supported range.");
        }

        if (totalCopies <= 0)
        {
            throw new DomainValidationException($"Book total copies must be greater than zero. Received: {totalCopies}.");
        }

        if (availableCopies < 0)
        {
            throw new DomainValidationException($"Book available copies cannot be negative. Received: {availableCopies}.");
        }

        if (availableCopies > totalCopies)
        {
            throw new DomainValidationException($"Book available copies ({availableCopies}) cannot exceed total copies ({totalCopies}).");
        }

        Title = title.Trim();
        Author = author.Trim();
        Isbn = isbn.Trim();
        PublicationYear = publicationYear;
        TotalCopies = totalCopies;
        AvailableCopies = availableCopies;

        if (touch)
        {
            Touch();
        }
    }
}
