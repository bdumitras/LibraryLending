namespace LibraryLending.UnitTests.Domain.Services;

using LibraryLending.Domain.Entities;
using LibraryLending.Domain.Enums;
using LibraryLending.Domain.Exceptions;
using LibraryLending.Domain.Services;

public class LoanDomainServiceTests
{
    private readonly LoanDomainService _service = new();

    [Fact]
    public void CreateLoan_Should_Decrement_Book_Availability_And_Return_Active_Loan()
    {
        var book = new Book("Domain-Driven Design", "Eric Evans", "9780321125217", 2003, 5, 2);
        var member = new Member("Jane Doe", "jane@example.com", true);
        var loanDateUtc = new DateTime(2026, 4, 14, 10, 0, 0, DateTimeKind.Utc);
        var dueDateUtc = loanDateUtc.AddDays(14);

        var loan = _service.CreateLoan(book, member, loanDateUtc, dueDateUtc);

        Assert.Equal(book.Id, loan.BookId);
        Assert.Equal(member.Id, loan.MemberId);
        Assert.Equal(LoanStatus.Active, loan.Status);
        Assert.Equal(1, book.AvailableCopies);
    }

    [Fact]
    public void CreateLoan_Should_Throw_When_Member_Is_Inactive()
    {
        var book = new Book("Domain-Driven Design", "Eric Evans", "9780321125217", 2003, 5, 2);
        var member = new Member("Jane Doe", "jane@example.com", false);
        var loanDateUtc = new DateTime(2026, 4, 14, 10, 0, 0, DateTimeKind.Utc);
        var dueDateUtc = loanDateUtc.AddDays(14);

        Assert.Throws<InactiveMemberCannotBorrowException>(() =>
            _service.CreateLoan(book, member, loanDateUtc, dueDateUtc));
    }

    [Fact]
    public void CreateLoan_Should_Throw_When_Book_Has_No_Available_Copies()
    {
        var book = new Book("Domain-Driven Design", "Eric Evans", "9780321125217", 2003, 5, 0);
        var member = new Member("Jane Doe", "jane@example.com", true);
        var loanDateUtc = new DateTime(2026, 4, 14, 10, 0, 0, DateTimeKind.Utc);
        var dueDateUtc = loanDateUtc.AddDays(14);

        Assert.Throws<BookHasNoAvailableCopiesException>(() =>
            _service.CreateLoan(book, member, loanDateUtc, dueDateUtc));
    }

    [Fact]
    public void ReturnLoan_Should_Mark_Loan_As_Returned_And_Increment_Book_Availability()
    {
        var book = new Book("Domain-Driven Design", "Eric Evans", "9780321125217", 2003, 5, 1);
        var member = new Member("Jane Doe", "jane@example.com", true);
        var loanDateUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var loan = _service.CreateLoan(book, member, loanDateUtc, loanDateUtc.AddDays(14));
        var returnedAtUtc = loanDateUtc.AddDays(2);

        _service.ReturnLoan(book, loan, returnedAtUtc);

        Assert.Equal(LoanStatus.Returned, loan.Status);
        Assert.Equal(returnedAtUtc, loan.ReturnedAtUtc);
        Assert.Equal(1, book.AvailableCopies);
    }
}
