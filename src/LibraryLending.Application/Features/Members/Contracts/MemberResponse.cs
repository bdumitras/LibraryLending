namespace LibraryLending.Application.Features.Members.Contracts;

public sealed record MemberResponse
{
    public required Guid Id { get; init; }

    public required string FullName { get; init; }

    public required string Email { get; init; }

    public required bool IsActive { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }
}
