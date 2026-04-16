using FluentValidation;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Extensions;
using LibraryLending.Application.Common.Logging;
using LibraryLending.Application.Features.Books.Exceptions;
using Microsoft.Extensions.Logging;

namespace LibraryLending.Application.Features.Books.Commands.DeleteBook;

public sealed class DeleteBookCommandHandler : ICommandHandler<DeleteBookCommand>
{
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<DeleteBookCommand> _validator;
    private readonly ILogger<DeleteBookCommandHandler> _logger;

    public DeleteBookCommandHandler(
        IBookRepository bookRepository,
        IUnitOfWork unitOfWork,
        IValidator<DeleteBookCommand> validator,
        ILogger<DeleteBookCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task Handle(DeleteBookCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var book = await _bookRepository.GetByIdAsync(command.Id, cancellationToken);
        if (book is null)
        {
            throw new BookNotFoundException(command.Id);
        }

        _bookRepository.Remove(book);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            ApplicationLogEvents.BookDeleted,
            "Deleted book {BookId} with ISBN {Isbn}.",
            book.Id,
            book.Isbn);
    }
}
