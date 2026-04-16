namespace LibraryLending.Application.Features.Members.Contracts;

public sealed record UpdateMemberRequest
{
    public required string FullName { get; init; }

    public required string Email { get; init; }

    public required bool IsActive { get; init; }
}
