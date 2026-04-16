using LibraryLending.Application.Common.Models;
using LibraryLending.Domain.Enums;

namespace LibraryLending.Application.Features.Loans.Models;

public sealed record LoanListFilter : PagedRequest
{
    public string? SearchTerm { get; init; }

    public Guid? BookId { get; init; }

    public Guid? MemberId { get; init; }

    public LoanStatus? Status { get; init; }

    public DateTime? LoanDateFromUtc { get; init; }

    public DateTime? LoanDateToUtc { get; init; }

    public DateTime? DueDateFromUtc { get; init; }

    public DateTime? DueDateToUtc { get; init; }

    public string? SortBy { get; init; }

    public SortDirection SortDirection { get; init; } = SortDirection.Desc;
}
