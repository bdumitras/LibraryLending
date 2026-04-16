using LibraryLending.Application.Common.Models;

namespace LibraryLending.Application.Features.Members.Models;

public sealed record MemberListFilter : PagedRequest
{
    public string? SearchTerm { get; init; }

    public string? FullName { get; init; }

    public string? Email { get; init; }

    public bool? IsActive { get; init; }

    public string? SortBy { get; init; }

    public SortDirection SortDirection { get; init; } = SortDirection.Asc;
}
