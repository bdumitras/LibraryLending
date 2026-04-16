namespace LibraryLending.Application.Features.Books.Contracts;

public sealed record BookResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Author { get; init; }

    public required string Isbn { get; init; }

    public required int PublicationYear { get; init; }

    public required int TotalCopies { get; init; }

    public required int AvailableCopies { get; init; }

    public required bool HasAvailableCopies { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }
}
