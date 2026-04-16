namespace LibraryLending.Domain.Services;

using LibraryLending.Domain.Entities;

public sealed class LoanDomainService
{
    public Loan CreateLoan(Book book, Member member, DateTime loanDateUtc, DateTime dueDateUtc)
    {
        ArgumentNullException.ThrowIfNull(book);
        ArgumentNullException.ThrowIfNull(member);

        member.EnsureCanBorrow();

        var loan = Loan.Create(book.Id, member.Id, loanDateUtc, dueDateUtc);

        book.BorrowCopy();

        return loan;
    }

    public void ReturnLoan(Book book, Loan loan, DateTime returnedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(book);
        ArgumentNullException.ThrowIfNull(loan);

        loan.Return(returnedAtUtc);
        book.ReturnCopy();
    }
}
