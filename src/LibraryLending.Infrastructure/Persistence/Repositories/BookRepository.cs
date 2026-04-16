using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Books.Models;
using LibraryLending.Domain.Entities;
using LibraryLending.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LibraryLending.Infrastructure.Persistence.Repositories;

public sealed class BookRepository : IBookRepository
{
    private readonly ApplicationDbContext _dbContext;

    public BookRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Books.FirstOrDefaultAsync(book => book.Id == id, cancellationToken);
    }

    public Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
    {
        var normalizedIsbn = isbn.Trim();
        return _dbContext.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(book => book.Isbn == normalizedIsbn, cancellationToken);
    }

    public Task<bool> ExistsByIsbnAsync(string isbn, Guid? excludingId = null, CancellationToken cancellationToken = default)
    {
        var normalizedIsbn = isbn.Trim();

        return _dbContext.Books.AnyAsync(
            book => book.Isbn == normalizedIsbn && (!excludingId.HasValue || book.Id != excludingId.Value),
            cancellationToken);
    }

    public Task<PagedResponse<Book>> GetPagedAsync(BookListFilter filter, CancellationToken cancellationToken = default)
    {
        IQueryable<Book> query = _dbContext.Books.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var pattern = $"%{filter.SearchTerm.Trim()}%";
            query = query.Where(book =>
                EF.Functions.ILike(book.Title, pattern) ||
                EF.Functions.ILike(book.Author, pattern) ||
                EF.Functions.ILike(book.Isbn, pattern));
        }

        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            var pattern = $"%{filter.Title.Trim()}%";
            query = query.Where(book => EF.Functions.ILike(book.Title, pattern));
        }

        if (!string.IsNullOrWhiteSpace(filter.Author))
        {
            var pattern = $"%{filter.Author.Trim()}%";
            query = query.Where(book => EF.Functions.ILike(book.Author, pattern));
        }

        if (!string.IsNullOrWhiteSpace(filter.Isbn))
        {
            var normalizedIsbn = filter.Isbn.Trim();
            query = query.Where(book => book.Isbn == normalizedIsbn);
        }

        if (filter.PublicationYearFrom.HasValue)
        {
            query = query.Where(book => book.PublicationYear >= filter.PublicationYearFrom.Value);
        }

        if (filter.PublicationYearTo.HasValue)
        {
            query = query.Where(book => book.PublicationYear <= filter.PublicationYearTo.Value);
        }

        if (filter.IsAvailable.HasValue)
        {
            query = filter.IsAvailable.Value
                ? query.Where(book => book.AvailableCopies > 0)
                : query.Where(book => book.AvailableCopies == 0);
        }

        query = ApplyOrdering(query, filter);

        return query.ToPagedResponseAsync(filter.Page, filter.PageSize, cancellationToken);
    }

    public Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        return _dbContext.Books.AddAsync(book, cancellationToken).AsTask();
    }

    public void Update(Book book)
    {
        _dbContext.Books.Update(book);
    }

    public void Remove(Book book)
    {
        _dbContext.Books.Remove(book);
    }

    private static IQueryable<Book> ApplyOrdering(IQueryable<Book> query, BookListFilter filter)
    {
        var sortBy = filter.SortBy?.Trim().ToLowerInvariant();
        var descending = filter.SortDirection == SortDirection.Desc;

        return (sortBy, descending) switch
        {
            ("title", true) => query.OrderByDescending(book => book.Title).ThenByDescending(book => book.Author).ThenBy(book => book.Id),
            ("title", false) => query.OrderBy(book => book.Title).ThenBy(book => book.Author).ThenBy(book => book.Id),

            ("author", true) => query.OrderByDescending(book => book.Author).ThenByDescending(book => book.Title).ThenBy(book => book.Id),
            ("author", false) => query.OrderBy(book => book.Author).ThenBy(book => book.Title).ThenBy(book => book.Id),

            ("publicationyear", true) => query.OrderByDescending(book => book.PublicationYear).ThenBy(book => book.Title).ThenBy(book => book.Id),
            ("publicationyear", false) => query.OrderBy(book => book.PublicationYear).ThenBy(book => book.Title).ThenBy(book => book.Id),

            ("isbn", true) => query.OrderByDescending(book => book.Isbn).ThenBy(book => book.Title).ThenBy(book => book.Id),
            ("isbn", false) => query.OrderBy(book => book.Isbn).ThenBy(book => book.Title).ThenBy(book => book.Id),

            ("availablecopies", true) => query.OrderByDescending(book => book.AvailableCopies).ThenBy(book => book.Title).ThenBy(book => book.Id),
            ("availablecopies", false) => query.OrderBy(book => book.AvailableCopies).ThenBy(book => book.Title).ThenBy(book => book.Id),

            ("createdat", true) => query.OrderByDescending(book => book.CreatedAtUtc).ThenBy(book => book.Title).ThenBy(book => book.Id),
            ("createdat", false) => query.OrderBy(book => book.CreatedAtUtc).ThenBy(book => book.Title).ThenBy(book => book.Id),

            ("updatedat", true) => query.OrderByDescending(book => book.UpdatedAtUtc).ThenByDescending(book => book.CreatedAtUtc).ThenBy(book => book.Id),
            ("updatedat", false) => query.OrderBy(book => book.UpdatedAtUtc).ThenBy(book => book.CreatedAtUtc).ThenBy(book => book.Id),

            (_, true) => query.OrderByDescending(book => book.Title).ThenByDescending(book => book.Author).ThenBy(book => book.Id),
            _ => query.OrderBy(book => book.Title).ThenBy(book => book.Author).ThenBy(book => book.Id)
        };
    }
}
