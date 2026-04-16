using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Books.Models;
using LibraryLending.Domain.Entities;

namespace LibraryLending.Application.Common.Abstractions.Persistence;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default);

    Task<bool> ExistsByIsbnAsync(string isbn, Guid? excludingId = null, CancellationToken cancellationToken = default);

    Task<PagedResponse<Book>> GetPagedAsync(BookListFilter filter, CancellationToken cancellationToken = default);

    Task AddAsync(Book book, CancellationToken cancellationToken = default);

    void Update(Book book);

    void Remove(Book book);
}
