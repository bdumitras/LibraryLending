using LibraryLending.Domain.Enums;

namespace LibraryLending.Application.Features.Loans.Contracts;

public sealed record LoanResponse
{
    public required Guid Id { get; init; }

    public required Guid BookId { get; init; }

    public required Guid MemberId { get; init; }

    public required DateTime LoanDateUtc { get; init; }

    public required DateTime DueDateUtc { get; init; }

    public DateTime? ReturnedAtUtc { get; init; }

    public required LoanStatus Status { get; init; }

    public required bool IsActive { get; init; }

    public required bool IsReturned { get; init; }

    public required bool IsOverdue { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }
}
