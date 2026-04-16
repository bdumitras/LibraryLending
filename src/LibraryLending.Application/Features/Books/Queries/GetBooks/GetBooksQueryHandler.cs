using FluentValidation;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Extensions;
using LibraryLending.Application.Common.Logging;
using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Books.Mappings;
using Microsoft.Extensions.Logging;

namespace LibraryLending.Application.Features.Books.Queries.GetBooks;

public sealed class GetBooksQueryHandler : IQueryHandler<GetBooksQuery, PagedResponse<Contracts.BookResponse>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IValidator<GetBooksQuery> _validator;
    private readonly ILogger<GetBooksQueryHandler> _logger;

    public GetBooksQueryHandler(
        IBookRepository bookRepository,
        IValidator<GetBooksQuery> validator,
        ILogger<GetBooksQueryHandler> logger)
    {
        _bookRepository = bookRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<PagedResponse<Contracts.BookResponse>> Handle(GetBooksQuery query, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var pagedBooks = await _bookRepository.GetPagedAsync(query.Filter, cancellationToken);

        _logger.LogDebug(
            ApplicationLogEvents.BooksListed,
            "Retrieved books page {Page} with page size {PageSize}. TotalCount {TotalCount}. SearchTerm {SearchTerm}. Isbn {Isbn}. SortBy {SortBy}. SortDirection {SortDirection}.",
            pagedBooks.Page,
            pagedBooks.PageSize,
            pagedBooks.TotalCount,
            query.Filter.SearchTerm,
            query.Filter.Isbn,
            query.Filter.SortBy,
            query.Filter.SortDirection);

        return pagedBooks.Map(book => book.ToResponse());
    }
}
