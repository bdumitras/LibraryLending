namespace LibraryLending.UnitTests.Application.Handlers.Loans;

using FluentValidation;
using LibraryLending.Application.Features.Loans.Commands.ReturnLoan;
using LibraryLending.Application.Features.Loans.Contracts;
using LibraryLending.Application.Features.Loans.Exceptions;
using LibraryLending.Domain.Entities;
using LibraryLending.Domain.Services;
using LibraryLending.UnitTests.Application.Handlers.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

public class ReturnLoanCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Loan_Increment_AvailableCopies_And_Save()
    {
        var book = new Book("Clean Code", "Robert C. Martin", "9780132350884", 2008, 3, 2);
        var loan = Loan.Create(
            book.Id,
            Guid.NewGuid(),
            new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 15, 12, 0, 0, DateTimeKind.Utc));

        var bookRepository = new InMemoryBookRepository(new[] { book });
        var loanRepository = new InMemoryLoanRepository(new[] { loan });
        var unitOfWork = new RecordingUnitOfWork();
        var validator = new InlineValidator<ReturnLoanCommand>();
        var handler = new ReturnLoanCommandHandler(
            bookRepository,
            loanRepository,
            unitOfWork,
            new LoanDomainService(),
            validator,
            NullLogger<ReturnLoanCommandHandler>.Instance);

        var returnedAt = new DateTime(2026, 4, 10, 12, 0, 0, DateTimeKind.Utc);
        var command = new ReturnLoanCommand(loan.Id, new ReturnLoanRequest
        {
            ReturnedAtUtc = returnedAt
        });

        var response = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(loan.Id, response.Id);
        Assert.Equal(returnedAt, response.ReturnedAtUtc);
        Assert.Equal(3, book.AvailableCopies);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.True(bookRepository.UpdateCalled);
        Assert.True(loanRepository.UpdateCalled);
        Assert.True(loan.IsReturned);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Loan_Does_Not_Exist()
    {
        var bookRepository = new InMemoryBookRepository();
        var loanRepository = new InMemoryLoanRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var validator = new InlineValidator<ReturnLoanCommand>();
        var handler = new ReturnLoanCommandHandler(
            bookRepository,
            loanRepository,
            unitOfWork,
            new LoanDomainService(),
            validator,
            NullLogger<ReturnLoanCommandHandler>.Instance);

        var command = new ReturnLoanCommand(Guid.NewGuid(), new ReturnLoanRequest
        {
            ReturnedAtUtc = new DateTime(2026, 4, 10, 12, 0, 0, DateTimeKind.Utc)
        });

        await Assert.ThrowsAsync<LoanNotFoundException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }
}
