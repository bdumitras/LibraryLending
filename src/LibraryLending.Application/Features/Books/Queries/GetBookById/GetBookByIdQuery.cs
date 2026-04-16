using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Features.Books.Contracts;

namespace LibraryLending.Application.Features.Books.Queries.GetBookById;

public sealed record GetBookByIdQuery(Guid Id) : IQuery<BookResponse>;
