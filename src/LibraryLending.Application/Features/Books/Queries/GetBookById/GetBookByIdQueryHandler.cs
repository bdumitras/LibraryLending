using FluentValidation;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Extensions;
using LibraryLending.Application.Common.Logging;
using LibraryLending.Application.Features.Books.Exceptions;
using LibraryLending.Application.Features.Books.Mappings;
using Microsoft.Extensions.Logging;

namespace LibraryLending.Application.Features.Books.Queries.GetBookById;

public sealed class GetBookByIdQueryHandler : IQueryHandler<GetBookByIdQuery, Contracts.BookResponse>
{
    private readonly IBookRepository _bookRepository;
    private readonly IValidator<GetBookByIdQuery> _validator;
    private readonly ILogger<GetBookByIdQueryHandler> _logger;

    public GetBookByIdQueryHandler(
        IBookRepository bookRepository,
        IValidator<GetBookByIdQuery> validator,
        ILogger<GetBookByIdQueryHandler> logger)
    {
        _bookRepository = bookRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Contracts.BookResponse> Handle(GetBookByIdQuery query, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var book = await _bookRepository.GetByIdAsync(query.Id, cancellationToken);
        if (book is null)
        {
            throw new BookNotFoundException(query.Id);
        }

        _logger.LogDebug(
            ApplicationLogEvents.BookRetrieved,
            "Retrieved book {BookId} with ISBN {Isbn}.",
            book.Id,
            book.Isbn);

        return book.ToResponse();
    }
}
