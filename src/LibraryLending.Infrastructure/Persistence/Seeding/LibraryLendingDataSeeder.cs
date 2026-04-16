using LibraryLending.Domain.Entities;
using LibraryLending.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibraryLending.Infrastructure.Persistence.Seeding;

public static class LibraryLendingDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext, ILogger logger, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(logger);

        var hasExistingData =
            await dbContext.Books.AnyAsync(cancellationToken) ||
            await dbContext.Members.AnyAsync(cancellationToken) ||
            await dbContext.Loans.AnyAsync(cancellationToken);

        if (hasExistingData)
        {
            logger.LogInformation("Skipping seed because the database already contains data.");
            return;
        }

        var books = CreateBooks();
        var members = CreateMembers();

        var nowUtc = DateTime.UtcNow;
        var activeMembers = members.Where(member => member.IsActive).ToList();
        var loanDomainService = new LoanDomainService();
        var loans = new List<Loan>();

        CreateCurrentLoans(books, activeMembers, loanDomainService, loans, nowUtc);
        CreateHistoricalLoans(books, activeMembers, loanDomainService, loans, nowUtc);

        await dbContext.Books.AddRangeAsync(books, cancellationToken);
        await dbContext.Members.AddRangeAsync(members, cancellationToken);
        await dbContext.Loans.AddRangeAsync(loans, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seeded database with {BookCount} books, {MemberCount} members, and {LoanCount} loans.",
            books.Count,
            members.Count,
            loans.Count);
    }

    private static List<Book> CreateBooks()
    {
        var titlePrefixes = new[]
        {
            "Silent", "Broken", "Hidden", "Last", "Electric", "Glass", "Northern", "Burning", "Fading", "Restless",
            "Shadow", "Golden", "Paper", "Dust", "Crimson", "Winter", "Midnight", "Storm", "Woven", "Concrete"
        };

        var titleSubjects = new[]
        {
            "Archive", "Garden", "City", "Library", "Signal", "Harbor", "Compass", "Map", "Season", "Bridge",
            "Code", "Room", "Lantern", "Memory", "Orbit", "River", "Machine", "Threshold", "Notebook", "Forest"
        };

        var authors = new[]
        {
            "Amelia Hart", "Jonas Reed", "Sofia Marin", "Elias Voss", "Nora Vale", "Milo Turner", "Iris Calder",
            "Dorian Pike", "Lena Cross", "Victor Hale", "Marta Iliev", "Anya Petrescu", "Samir Dobrev",
            "Clara Novak", "Leon Varga", "Talia Muresan", "Radu Enescu", "Mira Ionescu", "Petra Antonov",
            "Daniel Sava", "Elena Tudor", "Teodor Luca", "Ilinca Balan", "Matei Serban"
        };

        var books = new List<Book>(capacity: 120);

        for (var index = 1; index <= 120; index++)
        {
            var prefix = titlePrefixes[(index - 1) % titlePrefixes.Length];
            var subject = titleSubjects[((index - 1) / 2) % titleSubjects.Length];
            var author = authors[(index - 1) % authors.Length];
            var publicationYear = 1995 + ((index * 2) % 30);
            var totalCopies = 1 + (index % 5);

            books.Add(new Book(
                title: $"{prefix} {subject} {index:000}",
                author: author,
                isbn: GenerateIsbn(index),
                publicationYear: publicationYear,
                totalCopies: totalCopies,
                availableCopies: totalCopies));
        }

        return books;
    }

    private static List<Member> CreateMembers()
    {
        var names = new[]
        {
            "Alex Carter", "Bianca Muntean", "Calin Roman", "Daria Velea", "Emil Popa", "Flavia Rusu",
            "George Moga", "Hana Stan", "Ioan Pavel", "Julia Petcu", "Karim Hossam", "Larisa Nistor",
            "Mihai Lupu", "Nadia Toma", "Ovidiu Man", "Paula Seres", "Quinn Mercer", "Roxana Birsan",
            "Sergiu Fodor", "Teodora Miclea", "Ursula Dinu", "Vlad Bunea", "Yara Stoica", "Zeno Pavel"
        };

        var members = new List<Member>(capacity: names.Length);

        for (var index = 0; index < names.Length; index++)
        {
            var emailLocalPart = names[index]
                .ToLowerInvariant()
                .Replace(" ", ".")
                .Replace("ă", "a")
                .Replace("â", "a")
                .Replace("î", "i")
                .Replace("ș", "s")
                .Replace("ş", "s")
                .Replace("ț", "t")
                .Replace("ţ", "t");

            var isActive = index < 18;

            members.Add(new Member(
                fullName: names[index],
                email: $"{emailLocalPart}@example.com",
                isActive: isActive));
        }

        return members;
    }

    private static void CreateCurrentLoans(
        IReadOnlyList<Book> books,
        IReadOnlyList<Member> activeMembers,
        LoanDomainService loanDomainService,
        ICollection<Loan> loans,
        DateTime nowUtc)
    {
        for (var index = 0; index < 8; index++)
        {
            var loanDateUtc = nowUtc.Date.AddDays(-(index + 3));
            var dueDateUtc = nowUtc.Date.AddDays(7 + index);

            var loan = loanDomainService.CreateLoan(
                books[index],
                activeMembers[index],
                loanDateUtc,
                dueDateUtc);

            loans.Add(loan);
        }

        for (var index = 0; index < 6; index++)
        {
            var bookIndex = 8 + index;
            var loanDateUtc = nowUtc.Date.AddDays(-(18 + index * 2));
            var dueDateUtc = nowUtc.Date.AddDays(-(2 + index));

            var loan = loanDomainService.CreateLoan(
                books[bookIndex],
                activeMembers[8 + index],
                loanDateUtc,
                dueDateUtc);

            loan.RefreshStatus(nowUtc);
            loans.Add(loan);
        }
    }

    private static void CreateHistoricalLoans(
        IReadOnlyList<Book> books,
        IReadOnlyList<Member> activeMembers,
        LoanDomainService loanDomainService,
        ICollection<Loan> loans,
        DateTime nowUtc)
    {
        for (var index = 0; index < 10; index++)
        {
            var bookIndex = 20 + index;
            var memberIndex = (index * 2) % activeMembers.Count;

            var loanDateUtc = nowUtc.Date.AddDays(-(60 + index * 3));
            var dueDateUtc = loanDateUtc.AddDays(14);
            var returnedAtUtc = dueDateUtc.AddDays(1 + (index % 3));

            var loan = loanDomainService.CreateLoan(
                books[bookIndex],
                activeMembers[memberIndex],
                loanDateUtc,
                dueDateUtc);

            loanDomainService.ReturnLoan(books[bookIndex], loan, returnedAtUtc);
            loans.Add(loan);
        }
    }

    private static string GenerateIsbn(int index)
    {
        return $"978000000{index:0000}";
    }
}
