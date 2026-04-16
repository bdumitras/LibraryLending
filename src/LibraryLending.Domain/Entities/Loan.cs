namespace LibraryLending.Domain.Entities;

using LibraryLending.Domain.Common;
using LibraryLending.Domain.Enums;
using LibraryLending.Domain.Exceptions;

public class Loan : AuditableEntity
{
    private Loan()
    {
    }

    private Loan(Guid bookId, Guid memberId, DateTime loanDateUtc, DateTime dueDateUtc)
    {
        if (bookId == Guid.Empty)
        {
            throw new DomainValidationException("Loan book id is required.");
        }

        if (memberId == Guid.Empty)
        {
            throw new DomainValidationException("Loan member id is required.");
        }

        if (dueDateUtc <= loanDateUtc)
        {
            throw new InvalidLoanDateRangeException(loanDateUtc, dueDateUtc);
        }

        BookId = bookId;
        MemberId = memberId;
        LoanDateUtc = loanDateUtc;
        DueDateUtc = dueDateUtc;
        Status = LoanStatus.Active;
    }

    public Guid BookId { get; private set; }

    public Guid MemberId { get; private set; }

    public DateTime LoanDateUtc { get; private set; }

    public DateTime DueDateUtc { get; private set; }

    public DateTime? ReturnedAtUtc { get; private set; }

    public LoanStatus Status { get; private set; }

    public bool IsActive => Status == LoanStatus.Active;

    public bool IsReturned => Status == LoanStatus.Returned;

    public bool IsOverdue => Status == LoanStatus.Overdue;

    public static Loan Create(Guid bookId, Guid memberId, DateTime loanDateUtc, DateTime dueDateUtc)
    {
        return new Loan(bookId, memberId, loanDateUtc, dueDateUtc);
    }

    public void MarkAsOverdue(DateTime nowUtc)
    {
        if (IsReturned)
        {
            return;
        }

        if (nowUtc <= DueDateUtc)
        {
            return;
        }

        if (Status != LoanStatus.Overdue)
        {
            Status = LoanStatus.Overdue;
            Touch();
        }
    }

    public void RefreshStatus(DateTime nowUtc)
    {
        if (IsReturned)
        {
            return;
        }

        var nextStatus = nowUtc > DueDateUtc
            ? LoanStatus.Overdue
            : LoanStatus.Active;

        if (Status != nextStatus)
        {
            Status = nextStatus;
            Touch();
        }
    }

    public void Return(DateTime returnedAtUtc)
    {
        if (IsReturned)
        {
            throw new LoanAlreadyReturnedException(Id);
        }

        if (returnedAtUtc < LoanDateUtc)
        {
            throw new InvalidLoanReturnDateException(Id, LoanDateUtc, returnedAtUtc);
        }

        ReturnedAtUtc = returnedAtUtc;
        Status = LoanStatus.Returned;
        Touch();
    }
}
