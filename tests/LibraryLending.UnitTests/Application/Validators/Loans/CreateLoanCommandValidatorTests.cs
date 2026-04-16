namespace LibraryLending.UnitTests.Application.Validators.Loans;

using LibraryLending.Application.Features.Loans.Commands.CreateLoan;
using LibraryLending.Application.Features.Loans.Contracts;

public class CreateLoanCommandValidatorTests
{
    private readonly CreateLoanCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_Command_Is_Valid()
    {
        var loanDateUtc = new DateTime(2026, 4, 14, 10, 0, 0, DateTimeKind.Utc);
        var command = new CreateLoanCommand(new CreateLoanRequest
        {
            BookId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            LoanDateUtc = loanDateUtc,
            DueDateUtc = loanDateUtc.AddDays(14)
        });

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Should_Fail_When_Request_Is_Null()
    {
        var command = new CreateLoanCommand(null!);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Request");
    }

    [Fact]
    public void Validate_Should_Fail_When_BookId_Is_Empty()
    {
        var loanDateUtc = new DateTime(2026, 4, 14, 10, 0, 0, DateTimeKind.Utc);
        var command = new CreateLoanCommand(new CreateLoanRequest
        {
            BookId = Guid.Empty,
            MemberId = Guid.NewGuid(),
            LoanDateUtc = loanDateUtc,
            DueDateUtc = loanDateUtc.AddDays(14)
        });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Request.BookId");
    }

    [Fact]
    public void Validate_Should_Fail_When_LoanDate_Is_Default()
    {
        var command = new CreateLoanCommand(new CreateLoanRequest
        {
            BookId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            LoanDateUtc = default,
            DueDateUtc = new DateTime(2026, 4, 28, 10, 0, 0, DateTimeKind.Utc)
        });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Request.LoanDateUtc");
    }

    [Fact]
    public void Validate_Should_Fail_When_DueDate_Is_Not_After_LoanDate()
    {
        var loanDateUtc = new DateTime(2026, 4, 14, 10, 0, 0, DateTimeKind.Utc);
        var command = new CreateLoanCommand(new CreateLoanRequest
        {
            BookId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            LoanDateUtc = loanDateUtc,
            DueDateUtc = loanDateUtc
        });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "DueDateUtc must be greater than LoanDateUtc.");
    }
}
