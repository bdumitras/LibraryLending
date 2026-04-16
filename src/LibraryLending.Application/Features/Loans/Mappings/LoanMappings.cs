using LibraryLending.Application.Features.Loans.Contracts;
using LibraryLending.Domain.Entities;

namespace LibraryLending.Application.Features.Loans.Mappings;

public static class LoanMappings
{
    public static LoanResponse ToResponse(this Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        return new LoanResponse
        {
            Id = loan.Id,
            BookId = loan.BookId,
            MemberId = loan.MemberId,
            LoanDateUtc = loan.LoanDateUtc,
            DueDateUtc = loan.DueDateUtc,
            ReturnedAtUtc = loan.ReturnedAtUtc,
            Status = loan.Status,
            IsActive = loan.IsActive,
            IsReturned = loan.IsReturned,
            IsOverdue = loan.IsOverdue,
            CreatedAtUtc = loan.CreatedAtUtc,
            UpdatedAtUtc = loan.UpdatedAtUtc
        };
    }
}
