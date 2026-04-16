namespace LibraryLending.UnitTests.Application.Handlers.Loans;

using FluentValidation;
using LibraryLending.Application.Features.Loans.Commands.CreateLoan;
using LibraryLending.Application.Features.Loans.Contracts;
using LibraryLending.Application.Features.Members.Exceptions;
using LibraryLending.Domain.Entities;
using LibraryLending.Domain.Services;
using LibraryLending.UnitTests.Application.Handlers.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

public class CreateLoanCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Loan_Decrement_AvailableCopies_And_Save()
    {
        var book = new Book("Clean Code", "Robert C. Martin", "9780132350884", 2008, 3, 2);
        var member = new Member("Jane Doe", "jane@example.com", true);

        var bookRepository = new InMemoryBookRepository(new[] { book });
        var memberRepository = new InMemoryMemberRepository(new[] { member });
        var loanRepository = new InMemoryLoanRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var validator = new InlineValidator<CreateLoanCommand>();
        var handler = new CreateLoanCommandHandler(
            bookRepository,
            memberRepository,
            loanRepository,
            unitOfWork,
            new LoanDomainService(),
            validator,
            NullLogger<CreateLoanCommandHandler>.Instance);

        var loanDate = new DateTime(2026, 4, 14, 12, 0, 0, DateTimeKind.Utc);
        var dueDate = loanDate.AddDays(14);
        var command = new CreateLoanCommand(new CreateLoanRequest
        {
            BookId = book.Id,
            MemberId = member.Id,
            LoanDateUtc = loanDate,
            DueDateUtc = dueDate
        });

        var response = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(book.Id, response.BookId);
        Assert.Equal(member.Id, response.MemberId);
        Assert.Equal(1, book.AvailableCopies);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.True(bookRepository.UpdateCalled);

        var persistedLoan = await loanRepository.GetByIdAsync(response.Id, CancellationToken.None);
        Assert.NotNull(persistedLoan);
        Assert.Equal(loanDate, persistedLoan!.LoanDateUtc);
        Assert.Equal(dueDate, persistedLoan.DueDateUtc);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Member_Does_Not_Exist()
    {
        var book = new Book("Clean Code", "Robert C. Martin", "9780132350884", 2008, 3, 2);

        var bookRepository = new InMemoryBookRepository(new[] { book });
        var memberRepository = new InMemoryMemberRepository();
        var loanRepository = new InMemoryLoanRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var validator = new InlineValidator<CreateLoanCommand>();
        var handler = new CreateLoanCommandHandler(
            bookRepository,
            memberRepository,
            loanRepository,
            unitOfWork,
            new LoanDomainService(),
            validator,
            NullLogger<CreateLoanCommandHandler>.Instance);

        var command = new CreateLoanCommand(new CreateLoanRequest
        {
            BookId = book.Id,
            MemberId = Guid.NewGuid(),
            LoanDateUtc = new DateTime(2026, 4, 14, 12, 0, 0, DateTimeKind.Utc),
            DueDateUtc = new DateTime(2026, 4, 28, 12, 0, 0, DateTimeKind.Utc)
        });

        await Assert.ThrowsAsync<MemberNotFoundException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }
}
