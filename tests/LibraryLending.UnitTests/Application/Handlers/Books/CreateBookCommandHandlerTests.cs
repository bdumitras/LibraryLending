namespace LibraryLending.UnitTests.Application.Handlers.Books;

using FluentValidation;
using LibraryLending.Application.Features.Books.Commands.CreateBook;
using LibraryLending.Application.Features.Books.Contracts;
using LibraryLending.Application.Features.Books.Exceptions;
using LibraryLending.UnitTests.Application.Handlers.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

public class CreateBookCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Book_And_Save_When_Command_Is_Valid()
    {
        var bookRepository = new InMemoryBookRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var validator = new InlineValidator<CreateBookCommand>();
        var handler = new CreateBookCommandHandler(
            bookRepository,
            unitOfWork,
            validator,
            NullLogger<CreateBookCommandHandler>.Instance);

        var command = new CreateBookCommand(new CreateBookRequest
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            Isbn = " 9780132350884 ",
            PublicationYear = 2008,
            TotalCopies = 3,
            AvailableCopies = 2
        });

        var response = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Clean Code", response.Title);
        Assert.Equal("9780132350884", response.Isbn);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);

        var persisted = await bookRepository.GetByIdAsync(response.Id, CancellationToken.None);
        Assert.NotNull(persisted);
        Assert.Equal("9780132350884", persisted!.Isbn);
        Assert.Equal(2, persisted.AvailableCopies);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Isbn_Already_Exists()
    {
        var existingBook = new LibraryLending.Domain.Entities.Book(
            "Existing Book",
            "Author",
            "9780132350884",
            2000,
            2,
            2);

        var bookRepository = new InMemoryBookRepository(new[] { existingBook });
        var unitOfWork = new RecordingUnitOfWork();
        var validator = new InlineValidator<CreateBookCommand>();
        var handler = new CreateBookCommandHandler(
            bookRepository,
            unitOfWork,
            validator,
            NullLogger<CreateBookCommandHandler>.Instance);

        var command = new CreateBookCommand(new CreateBookRequest
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            Isbn = "9780132350884",
            PublicationYear = 2008,
            TotalCopies = 3,
            AvailableCopies = 2
        });

        await Assert.ThrowsAsync<BookIsbnAlreadyExistsException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }
}
