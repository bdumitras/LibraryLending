using LibraryLending.Domain.Entities;
using LibraryLending.Domain.Services;

namespace LibraryLending.IntegrationTests.Infrastructure;

public static class TestDataFactory
{
    public static Book CreateBook(
        string? title = null,
        string? author = null,
        string? isbn = null,
        int publicationYear = 2020,
        int totalCopies = 3,
        int availableCopies = 3)
    {
        return new Book(
            title ?? $"Book {Guid.NewGuid():N}",
            author ?? "Test Author",
            isbn ?? $"978{Random.Shared.NextInt64(1_000_000_000, 9_999_999_999)}",
            publicationYear,
            totalCopies,
            availableCopies);
    }

    public static Member CreateMember(
        string? fullName = null,
        string? email = null,
        bool isActive = true)
    {
        return new Member(
            fullName ?? $"Member {Guid.NewGuid():N}",
            email ?? $"member.{Guid.NewGuid():N}@example.com",
            isActive);
    }

    public static Loan CreateActiveLoan(
        Book book,
        Member member,
        DateTime? loanDateUtc = null,
        DateTime? dueDateUtc = null)
    {
        var nowUtc = DateTime.UtcNow;
        var effectiveLoanDateUtc = loanDateUtc ?? nowUtc.AddDays(-2);
        var effectiveDueDateUtc = dueDateUtc ?? nowUtc.AddDays(12);

        return new LoanDomainService().CreateLoan(book, member, effectiveLoanDateUtc, effectiveDueDateUtc);
    }

    public static Loan CreateReturnedLoan(
        Book book,
        Member member,
        DateTime? loanDateUtc = null,
        DateTime? dueDateUtc = null,
        DateTime? returnedAtUtc = null)
    {
        var nowUtc = DateTime.UtcNow;
        var effectiveLoanDateUtc = loanDateUtc ?? nowUtc.AddDays(-20);
        var effectiveDueDateUtc = dueDateUtc ?? nowUtc.AddDays(-6);
        var effectiveReturnedAtUtc = returnedAtUtc ?? nowUtc.AddDays(-3);

        var loanDomainService = new LoanDomainService();
        var loan = loanDomainService.CreateLoan(book, member, effectiveLoanDateUtc, effectiveDueDateUtc);
        loanDomainService.ReturnLoan(book, loan, effectiveReturnedAtUtc);

        return loan;
    }

    public static Loan CreateOverdueLoan(
        Book book,
        Member member,
        DateTime? loanDateUtc = null,
        DateTime? dueDateUtc = null)
    {
        var nowUtc = DateTime.UtcNow;
        var effectiveLoanDateUtc = loanDateUtc ?? nowUtc.AddDays(-15);
        var effectiveDueDateUtc = dueDateUtc ?? nowUtc.AddDays(-2);

        var loan = new LoanDomainService().CreateLoan(book, member, effectiveLoanDateUtc, effectiveDueDateUtc);
        loan.RefreshStatus(nowUtc);

        return loan;
    }
}
