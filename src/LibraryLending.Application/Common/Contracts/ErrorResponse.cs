namespace LibraryLending.Application.Common.Contracts;

public sealed record ErrorResponse
{
    public required string Type { get; init; }

    public required string Title { get; init; }

    public required int Status { get; init; }

    public string? Detail { get; init; }

    public string? Instance { get; init; }

    public string? ErrorCode { get; init; }

    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }
}
