using LibraryLending.Application.Common.Exceptions;

namespace LibraryLending.Application.Features.Loans.Exceptions;

public sealed class LoanNotFoundException : NotFoundException
{
    public LoanNotFoundException(Guid loanId)
        : base($"Loan '{loanId}' was not found.")
    {
    }
}
