using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Loans.Models;
using LibraryLending.Domain.Entities;

namespace LibraryLending.Application.Common.Abstractions.Persistence;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResponse<Loan>> GetPagedAsync(LoanListFilter filter, CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveLoanForBookAsync(Guid bookId, Guid? excludingLoanId = null, CancellationToken cancellationToken = default);

    Task AddAsync(Loan loan, CancellationToken cancellationToken = default);

    void Update(Loan loan);
}
