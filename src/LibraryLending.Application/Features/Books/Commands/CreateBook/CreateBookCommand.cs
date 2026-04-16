using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Features.Books.Contracts;

namespace LibraryLending.Application.Features.Books.Commands.CreateBook;

public sealed record CreateBookCommand(CreateBookRequest Request) : ICommand<BookResponse>;
