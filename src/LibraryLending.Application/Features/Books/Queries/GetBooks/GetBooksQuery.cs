using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Books.Contracts;
using LibraryLending.Application.Features.Books.Models;

namespace LibraryLending.Application.Features.Books.Queries.GetBooks;

public sealed record GetBooksQuery(BookListFilter Filter) : IQuery<PagedResponse<BookResponse>>;
