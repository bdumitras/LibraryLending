namespace LibraryLending.Application.Features.Members.Contracts;

public sealed record CreateMemberRequest
{
    public required string FullName { get; init; }

    public required string Email { get; init; }

    public bool IsActive { get; init; } = true;
}
