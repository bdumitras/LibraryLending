using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Loans.Models;
using LibraryLending.Domain.Entities;
using LibraryLending.Domain.Enums;
using LibraryLending.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LibraryLending.Infrastructure.Persistence.Repositories;

public sealed class LoanRepository : ILoanRepository
{
    private readonly ApplicationDbContext _dbContext;

    public LoanRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Loans.FirstOrDefaultAsync(loan => loan.Id == id, cancellationToken);
    }

    public Task<PagedResponse<Loan>> GetPagedAsync(LoanListFilter filter, CancellationToken cancellationToken = default)
    {
        IQueryable<Loan> query = _dbContext.Loans;
        var nowUtc = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var pattern = $"%{filter.SearchTerm.Trim()}%";
            query = query.Where(loan =>
                _dbContext.Books.Any(book =>
                    book.Id == loan.BookId &&
                    (EF.Functions.ILike(book.Title, pattern) || EF.Functions.ILike(book.Isbn, pattern))) ||
                _dbContext.Members.Any(member =>
                    member.Id == loan.MemberId &&
                    (EF.Functions.ILike(member.FullName, pattern) || EF.Functions.ILike(member.Email, pattern))));
        }

        if (filter.BookId.HasValue)
        {
            query = query.Where(loan => loan.BookId == filter.BookId.Value);
        }

        if (filter.MemberId.HasValue)
        {
            query = query.Where(loan => loan.MemberId == filter.MemberId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = filter.Status.Value switch
            {
                LoanStatus.Returned => query.Where(loan => loan.ReturnedAtUtc.HasValue),
                LoanStatus.Overdue => query.Where(loan => !loan.ReturnedAtUtc.HasValue && loan.DueDateUtc < nowUtc),
                _ => query.Where(loan => !loan.ReturnedAtUtc.HasValue && loan.DueDateUtc >= nowUtc)
            };
        }

        if (filter.LoanDateFromUtc.HasValue)
        {
            query = query.Where(loan => loan.LoanDateUtc >= filter.LoanDateFromUtc.Value);
        }

        if (filter.LoanDateToUtc.HasValue)
        {
            query = query.Where(loan => loan.LoanDateUtc <= filter.LoanDateToUtc.Value);
        }

        if (filter.DueDateFromUtc.HasValue)
        {
            query = query.Where(loan => loan.DueDateUtc >= filter.DueDateFromUtc.Value);
        }

        if (filter.DueDateToUtc.HasValue)
        {
            query = query.Where(loan => loan.DueDateUtc <= filter.DueDateToUtc.Value);
        }

        query = ApplyOrdering(query, filter);

        return query.ToPagedResponseAsync(filter.Page, filter.PageSize, cancellationToken);
    }

    public Task<bool> ExistsActiveLoanForBookAsync(Guid bookId, Guid? excludingLoanId = null, CancellationToken cancellationToken = default)
    {
        return _dbContext.Loans.AnyAsync(
            loan => loan.BookId == bookId
                    && !loan.ReturnedAtUtc.HasValue
                    && (!excludingLoanId.HasValue || loan.Id != excludingLoanId.Value),
            cancellationToken);
    }

    public Task AddAsync(Loan loan, CancellationToken cancellationToken = default)
    {
        return _dbContext.Loans.AddAsync(loan, cancellationToken).AsTask();
    }

    public void Update(Loan loan)
    {
        _dbContext.Loans.Update(loan);
    }

    private static IQueryable<Loan> ApplyOrdering(IQueryable<Loan> query, LoanListFilter filter)
    {
        var sortBy = filter.SortBy?.Trim().ToLowerInvariant();
        var descending = filter.SortDirection == SortDirection.Desc;

        return (sortBy, descending) switch
        {
            ("duedate", true) => query.OrderByDescending(loan => loan.DueDateUtc).ThenByDescending(loan => loan.LoanDateUtc).ThenBy(loan => loan.Id),
            ("duedate", false) => query.OrderBy(loan => loan.DueDateUtc).ThenBy(loan => loan.LoanDateUtc).ThenBy(loan => loan.Id),

            ("returnedat", true) => query.OrderByDescending(loan => loan.ReturnedAtUtc).ThenByDescending(loan => loan.LoanDateUtc).ThenBy(loan => loan.Id),
            ("returnedat", false) => query.OrderBy(loan => loan.ReturnedAtUtc).ThenBy(loan => loan.LoanDateUtc).ThenBy(loan => loan.Id),

            ("status", true) => query.OrderByDescending(loan => loan.Status).ThenByDescending(loan => loan.DueDateUtc).ThenBy(loan => loan.Id),
            ("status", false) => query.OrderBy(loan => loan.Status).ThenBy(loan => loan.DueDateUtc).ThenBy(loan => loan.Id),

            ("createdat", true) => query.OrderByDescending(loan => loan.CreatedAtUtc).ThenByDescending(loan => loan.LoanDateUtc).ThenBy(loan => loan.Id),
            ("createdat", false) => query.OrderBy(loan => loan.CreatedAtUtc).ThenBy(loan => loan.LoanDateUtc).ThenBy(loan => loan.Id),

            ("updatedat", true) => query.OrderByDescending(loan => loan.UpdatedAtUtc).ThenByDescending(loan => loan.CreatedAtUtc).ThenBy(loan => loan.Id),
            ("updatedat", false) => query.OrderBy(loan => loan.UpdatedAtUtc).ThenBy(loan => loan.CreatedAtUtc).ThenBy(loan => loan.Id),

            ("loandate", false) => query.OrderBy(loan => loan.LoanDateUtc).ThenBy(loan => loan.DueDateUtc).ThenBy(loan => loan.Id),
            _ => query.OrderByDescending(loan => loan.LoanDateUtc).ThenByDescending(loan => loan.CreatedAtUtc).ThenBy(loan => loan.Id)
        };
    }
}
