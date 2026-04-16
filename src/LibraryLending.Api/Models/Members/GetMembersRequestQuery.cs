using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Members.Models;

namespace LibraryLending.Api.Models.Members;

public sealed class GetMembersRequestQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? SearchTerm { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public bool? IsActive { get; set; }

    public string? SortBy { get; set; }

    public SortDirection SortDirection { get; set; } = SortDirection.Asc;

    public MemberListFilter ToFilter()
    {
        return new MemberListFilter
        {
            Page = Page,
            PageSize = PageSize,
            SearchTerm = SearchTerm,
            FullName = FullName,
            Email = Email,
            IsActive = IsActive,
            SortBy = SortBy,
            SortDirection = SortDirection
        };
    }
}
