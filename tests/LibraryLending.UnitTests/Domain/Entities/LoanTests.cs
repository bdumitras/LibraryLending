namespace LibraryLending.UnitTests.Domain.Entities;

using LibraryLending.Domain.Entities;
using LibraryLending.Domain.Enums;
using LibraryLending.Domain.Exceptions;

public class LoanTests
{
    [Fact]
    public void Create_Should_Set_Status_To_Active_When_Data_Is_Valid()
    {
        var loanDateUtc = new DateTime(2026, 4, 14, 10, 0, 0, DateTimeKind.Utc);
        var dueDateUtc = loanDateUtc.AddDays(14);

        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), loanDateUtc, dueDateUtc);

        Assert.Equal(LoanStatus.Active, loan.Status);
        Assert.Equal(loanDateUtc, loan.LoanDateUtc);
        Assert.Equal(dueDateUtc, loan.DueDateUtc);
        Assert.Null(loan.ReturnedAtUtc);
        Assert.True(loan.IsActive);
    }

    [Fact]
    public void Create_Should_Throw_When_DueDate_Is_Not_After_LoanDate()
    {
        var loanDateUtc = new DateTime(2026, 4, 14, 10, 0, 0, DateTimeKind.Utc);

        Assert.Throws<InvalidLoanDateRangeException>(() =>
            Loan.Create(Guid.NewGuid(), Guid.NewGuid(), loanDateUtc, loanDateUtc));
    }

    [Fact]
    public void Return_Should_Set_Status_To_Returned_And_ReturnedAtUtc()
    {
        var loanDateUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), loanDateUtc, loanDateUtc.AddDays(14));
        var returnedAtUtc = loanDateUtc.AddDays(3);

        loan.Return(returnedAtUtc);

        Assert.Equal(LoanStatus.Returned, loan.Status);
        Assert.Equal(returnedAtUtc, loan.ReturnedAtUtc);
        Assert.True(loan.IsReturned);
    }

    [Fact]
    public void Return_Should_Throw_When_Loan_Is_Already_Returned()
    {
        var loanDateUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), loanDateUtc, loanDateUtc.AddDays(14));
        loan.Return(loanDateUtc.AddDays(2));

        Assert.Throws<LoanAlreadyReturnedException>(() => loan.Return(loanDateUtc.AddDays(3)));
    }

    [Fact]
    public void Return_Should_Throw_When_ReturnDate_Is_Before_LoanDate()
    {
        var loanDateUtc = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc);
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), loanDateUtc, loanDateUtc.AddDays(14));

        Assert.Throws<InvalidLoanReturnDateException>(() => loan.Return(loanDateUtc.AddMinutes(-1)));
    }

    [Fact]
    public void RefreshStatus_Should_Set_Overdue_When_DueDate_Has_Passed()
    {
        var loanDateUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), loanDateUtc, loanDateUtc.AddDays(7));

        loan.RefreshStatus(loanDateUtc.AddDays(8));

        Assert.Equal(LoanStatus.Overdue, loan.Status);
        Assert.True(loan.IsOverdue);
    }

    [Fact]
    public void RefreshStatus_Should_Keep_Active_When_DueDate_Has_Not_Passed()
    {
        var loanDateUtc = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), loanDateUtc, loanDateUtc.AddDays(7));

        loan.RefreshStatus(loanDateUtc.AddDays(3));

        Assert.Equal(LoanStatus.Active, loan.Status);
        Assert.True(loan.IsActive);
    }
}
