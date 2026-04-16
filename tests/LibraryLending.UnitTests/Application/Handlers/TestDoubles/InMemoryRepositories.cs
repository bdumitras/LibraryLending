namespace LibraryLending.UnitTests.Application.Handlers.TestDoubles;

using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Books.Models;
using LibraryLending.Application.Features.Loans.Models;
using LibraryLending.Application.Features.Members.Models;
using LibraryLending.Domain.Entities;
using LibraryLending.Domain.Enums;

internal sealed class InMemoryBookRepository : IBookRepository
{
    private readonly Dictionary<Guid, Book> _books = new();

    public bool UpdateCalled { get; private set; }
    public bool RemoveCalled { get; private set; }

    public InMemoryBookRepository(params IEnumerable<Book> books)
    {
        foreach (var book in books)
        {
            _books[book.Id] = book;
        }
    }

    public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_books.TryGetValue(id, out var book) ? book : null);

    public Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
        => Task.FromResult(_books.Values.FirstOrDefault(x => string.Equals(x.Isbn, isbn, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> ExistsByIsbnAsync(string isbn, Guid? excludingId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_books.Values.Any(x =>
            string.Equals(x.Isbn, isbn, StringComparison.OrdinalIgnoreCase)
            && (!excludingId.HasValue || x.Id != excludingId.Value)));

    public Task<PagedResponse<Book>> GetPagedAsync(BookListFilter filter, CancellationToken cancellationToken = default)
        => Task.FromResult(new PagedResponse<Book>
        {
            Items = _books.Values.ToList(),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = _books.Count
        });

    public Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        _books[book.Id] = book;
        return Task.CompletedTask;
    }

    public void Update(Book book)
    {
        UpdateCalled = true;
        _books[book.Id] = book;
    }

    public void Remove(Book book)
    {
        RemoveCalled = true;
        _books.Remove(book.Id);
    }
}

internal sealed class InMemoryMemberRepository : IMemberRepository
{
    private readonly Dictionary<Guid, Member> _members = new();

    public bool UpdateCalled { get; private set; }
    public bool RemoveCalled { get; private set; }

    public InMemoryMemberRepository(params IEnumerable<Member> members)
    {
        foreach (var member in members)
        {
            _members[member.Id] = member;
        }
    }

    public Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_members.TryGetValue(id, out var member) ? member : null);

    public Task<Member?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => Task.FromResult(_members.Values.FirstOrDefault(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> ExistsByEmailAsync(string email, Guid? excludingId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_members.Values.Any(x =>
            string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase)
            && (!excludingId.HasValue || x.Id != excludingId.Value)));

    public Task<PagedResponse<Member>> GetPagedAsync(MemberListFilter filter, CancellationToken cancellationToken = default)
        => Task.FromResult(new PagedResponse<Member>
        {
            Items = _members.Values.ToList(),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = _members.Count
        });

    public Task AddAsync(Member member, CancellationToken cancellationToken = default)
    {
        _members[member.Id] = member;
        return Task.CompletedTask;
    }

    public void Update(Member member)
    {
        UpdateCalled = true;
        _members[member.Id] = member;
    }

    public void Remove(Member member)
    {
        RemoveCalled = true;
        _members.Remove(member.Id);
    }
}

internal sealed class InMemoryLoanRepository : ILoanRepository
{
    private readonly Dictionary<Guid, Loan> _loans = new();

    public bool UpdateCalled { get; private set; }

    public InMemoryLoanRepository(params IEnumerable<Loan> loans)
    {
        foreach (var loan in loans)
        {
            _loans[loan.Id] = loan;
        }
    }

    public Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_loans.TryGetValue(id, out var loan) ? loan : null);

    public Task<PagedResponse<Loan>> GetPagedAsync(LoanListFilter filter, CancellationToken cancellationToken = default)
        => Task.FromResult(new PagedResponse<Loan>
        {
            Items = _loans.Values.ToList(),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = _loans.Count
        });

    public Task<bool> ExistsActiveLoanForBookAsync(Guid bookId, Guid? excludingLoanId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_loans.Values.Any(x =>
            x.BookId == bookId
            && (x.Status == LoanStatus.Active || x.Status == LoanStatus.Overdue)
            && (!excludingLoanId.HasValue || x.Id != excludingLoanId.Value)));

    public Task AddAsync(Loan loan, CancellationToken cancellationToken = default)
    {
        _loans[loan.Id] = loan;
        return Task.CompletedTask;
    }

    public void Update(Loan loan)
    {
        UpdateCalled = true;
        _loans[loan.Id] = loan;
    }
}

internal sealed class RecordingUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(1);
    }
}
