using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Loans.Models;
using LibraryLending.Domain.Enums;

namespace LibraryLending.Api.Models.Loans;

public sealed class GetLoansRequestQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? SearchTerm { get; set; }

    public Guid? BookId { get; set; }

    public Guid? MemberId { get; set; }

    public LoanStatus? Status { get; set; }

    public DateTime? LoanDateFromUtc { get; set; }

    public DateTime? LoanDateToUtc { get; set; }

    public DateTime? DueDateFromUtc { get; set; }

    public DateTime? DueDateToUtc { get; set; }

    public string? SortBy { get; set; }

    public SortDirection SortDirection { get; set; } = SortDirection.Desc;

    public LoanListFilter ToFilter()
    {
        return new LoanListFilter
        {
            Page = Page,
            PageSize = PageSize,
            SearchTerm = SearchTerm,
            BookId = BookId,
            MemberId = MemberId,
            Status = Status,
            LoanDateFromUtc = LoanDateFromUtc,
            LoanDateToUtc = LoanDateToUtc,
            DueDateFromUtc = DueDateFromUtc,
            DueDateToUtc = DueDateToUtc,
            SortBy = SortBy,
            SortDirection = SortDirection
        };
    }
}
