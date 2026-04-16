namespace LibraryLending.Domain.Exceptions;

public sealed class InvalidLoanDateRangeException : DomainException
{
    public InvalidLoanDateRangeException(DateTime loanDateUtc, DateTime dueDateUtc)
        : base($"Loan due date '{dueDateUtc:O}' must be after loan date '{loanDateUtc:O}'.")
    {
        LoanDateUtc = loanDateUtc;
        DueDateUtc = dueDateUtc;
    }

    public DateTime LoanDateUtc { get; }

    public DateTime DueDateUtc { get; }
}
