using LibraryLending.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryLending.IntegrationTests.Infrastructure;

public static class TestDataSeeder
{
    public static async Task<SeededLibraryScenario> SeedBasicScenarioAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var availableBook = TestDataFactory.CreateBook(
            title: "Available Book",
            author: "Scenario Author",
            isbn: "9781000000001",
            publicationYear: 2020,
            totalCopies: 4,
            availableCopies: 4);

        var loanedBook = TestDataFactory.CreateBook(
            title: "Loaned Book",
            author: "Scenario Author",
            isbn: "9781000000002",
            publicationYear: 2021,
            totalCopies: 2,
            availableCopies: 2);

        var returnedBook = TestDataFactory.CreateBook(
            title: "Returned Book",
            author: "Scenario Author",
            isbn: "9781000000003",
            publicationYear: 2022,
            totalCopies: 1,
            availableCopies: 1);

        var activeMember = TestDataFactory.CreateMember(
            fullName: "Active Member",
            email: "active.member@example.com",
            isActive: true);

        var inactiveMember = TestDataFactory.CreateMember(
            fullName: "Inactive Member",
            email: "inactive.member@example.com",
            isActive: false);

        var returningMember = TestDataFactory.CreateMember(
            fullName: "Returning Member",
            email: "returning.member@example.com",
            isActive: true);

        var activeLoan = TestDataFactory.CreateActiveLoan(loanedBook, activeMember);
        var returnedLoan = TestDataFactory.CreateReturnedLoan(returnedBook, returningMember);

        await dbContext.Books.AddRangeAsync([availableBook, loanedBook, returnedBook], cancellationToken);
        await dbContext.Members.AddRangeAsync([activeMember, inactiveMember, returningMember], cancellationToken);
        await dbContext.Loans.AddRangeAsync([activeLoan, returnedLoan], cancellationToken);

        return new SeededLibraryScenario(
            availableBook.Id,
            loanedBook.Id,
            returnedBook.Id,
            activeMember.Id,
            inactiveMember.Id,
            returningMember.Id,
            activeLoan.Id,
            returnedLoan.Id);
    }

    public static async Task<int> CountAllRowsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var books = await dbContext.Books.CountAsync(cancellationToken);
        var members = await dbContext.Members.CountAsync(cancellationToken);
        var loans = await dbContext.Loans.CountAsync(cancellationToken);

        return books + members + loans;
    }
}
