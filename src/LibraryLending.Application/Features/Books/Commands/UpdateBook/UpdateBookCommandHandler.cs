using FluentValidation;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Extensions;
using LibraryLending.Application.Common.Logging;
using LibraryLending.Application.Features.Books.Exceptions;
using LibraryLending.Application.Features.Books.Mappings;
using Microsoft.Extensions.Logging;

namespace LibraryLending.Application.Features.Books.Commands.UpdateBook;

public sealed class UpdateBookCommandHandler : ICommandHandler<UpdateBookCommand, Contracts.BookResponse>
{
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateBookCommand> _validator;
    private readonly ILogger<UpdateBookCommandHandler> _logger;

    public UpdateBookCommandHandler(
        IBookRepository bookRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateBookCommand> validator,
        ILogger<UpdateBookCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Contracts.BookResponse> Handle(UpdateBookCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var book = await _bookRepository.GetByIdAsync(command.Id, cancellationToken);
        if (book is null)
        {
            throw new BookNotFoundException(command.Id);
        }

        var normalizedIsbn = command.Request.Isbn.Trim();
        var duplicateIsbn = await _bookRepository.ExistsByIsbnAsync(normalizedIsbn, command.Id, cancellationToken);
        if (duplicateIsbn)
        {
            throw new BookIsbnAlreadyExistsException(normalizedIsbn);
        }

        book.UpdateDetails(
            command.Request.Title,
            command.Request.Author,
            normalizedIsbn,
            command.Request.PublicationYear,
            command.Request.TotalCopies,
            command.Request.AvailableCopies);

        _bookRepository.Update(book);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            ApplicationLogEvents.BookUpdated,
            "Updated book {BookId} with ISBN {Isbn}. TotalCopies {TotalCopies}. AvailableCopies {AvailableCopies}.",
            book.Id,
            book.Isbn,
            book.TotalCopies,
            book.AvailableCopies);

        return book.ToResponse();
    }
}
