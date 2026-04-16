namespace LibraryLending.Domain.Exceptions;

public sealed class LoanAlreadyReturnedException : DomainException
{
    public LoanAlreadyReturnedException(Guid loanId)
        : base($"Loan '{loanId}' has already been returned.")
    {
        LoanId = loanId;
    }

    public Guid LoanId { get; }
}
