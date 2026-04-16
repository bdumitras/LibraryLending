namespace LibraryLending.Application.Features.Loans.Contracts;

public sealed record CreateLoanRequest
{
    public required Guid BookId { get; init; }

    public required Guid MemberId { get; init; }

    public required DateTime LoanDateUtc { get; init; }

    public required DateTime DueDateUtc { get; init; }
}
