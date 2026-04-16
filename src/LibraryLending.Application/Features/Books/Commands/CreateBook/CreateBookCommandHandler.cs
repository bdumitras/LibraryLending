using FluentValidation;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Extensions;
using LibraryLending.Application.Common.Logging;
using LibraryLending.Application.Features.Books.Exceptions;
using LibraryLending.Application.Features.Books.Mappings;
using LibraryLending.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LibraryLending.Application.Features.Books.Commands.CreateBook;

public sealed class CreateBookCommandHandler : ICommandHandler<CreateBookCommand, Contracts.BookResponse>
{
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateBookCommand> _validator;
    private readonly ILogger<CreateBookCommandHandler> _logger;

    public CreateBookCommandHandler(
        IBookRepository bookRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateBookCommand> validator,
        ILogger<CreateBookCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Contracts.BookResponse> Handle(CreateBookCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var normalizedIsbn = command.Request.Isbn.Trim();
        var exists = await _bookRepository.ExistsByIsbnAsync(normalizedIsbn, cancellationToken: cancellationToken);
        if (exists)
        {
            throw new BookIsbnAlreadyExistsException(normalizedIsbn);
        }

        var book = new Book(
            command.Request.Title,
            command.Request.Author,
            normalizedIsbn,
            command.Request.PublicationYear,
            command.Request.TotalCopies,
            command.Request.AvailableCopies);

        await _bookRepository.AddAsync(book, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            ApplicationLogEvents.BookCreated,
            "Created book {BookId} with ISBN {Isbn}. TotalCopies {TotalCopies}. AvailableCopies {AvailableCopies}.",
            book.Id,
            book.Isbn,
            book.TotalCopies,
            book.AvailableCopies);

        return book.ToResponse();
    }
}
