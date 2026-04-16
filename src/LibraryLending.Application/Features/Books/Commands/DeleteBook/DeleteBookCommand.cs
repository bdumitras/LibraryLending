using LibraryLending.Application.Common.Abstractions.Messaging;

namespace LibraryLending.Application.Features.Books.Commands.DeleteBook;

public sealed record DeleteBookCommand(Guid Id) : ICommand;
