using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Features.Books.Contracts;

namespace LibraryLending.Application.Features.Books.Commands.UpdateBook;

public sealed record UpdateBookCommand(Guid Id, UpdateBookRequest Request) : ICommand<BookResponse>;
