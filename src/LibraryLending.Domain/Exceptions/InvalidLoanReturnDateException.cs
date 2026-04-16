namespace LibraryLending.Domain.Exceptions;

public sealed class InvalidLoanReturnDateException : DomainException
{
    public InvalidLoanReturnDateException(Guid loanId, DateTime loanDateUtc, DateTime returnedAtUtc)
        : base($"Loan '{loanId}' cannot be returned at '{returnedAtUtc:O}' because it is before the loan date '{loanDateUtc:O}'.")
    {
        LoanId = loanId;
        LoanDateUtc = loanDateUtc;
        ReturnedAtUtc = returnedAtUtc;
    }

    public Guid LoanId { get; }

    public DateTime LoanDateUtc { get; }

    public DateTime ReturnedAtUtc { get; }
}
