namespace LibraryLending.Application.Features.Books.Contracts;

public sealed record CreateBookRequest
{
    public required string Title { get; init; }

    public required string Author { get; init; }

    public required string Isbn { get; init; }

    public required int PublicationYear { get; init; }

    public required int TotalCopies { get; init; }

    public required int AvailableCopies { get; init; }
}
