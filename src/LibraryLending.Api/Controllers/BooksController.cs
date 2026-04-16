using LibraryLending.Api.Models.Books;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Contracts;
using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Books.Commands.CreateBook;
using LibraryLending.Application.Features.Books.Commands.DeleteBook;
using LibraryLending.Application.Features.Books.Commands.UpdateBook;
using LibraryLending.Application.Features.Books.Contracts;
using LibraryLending.Application.Features.Books.Queries.GetBookById;
using LibraryLending.Application.Features.Books.Queries.GetBooks;
using Microsoft.AspNetCore.Mvc;

namespace LibraryLending.Api.Controllers;

/// <summary>
/// Exposes CRUD endpoints for library books.
/// </summary>
public sealed class BooksController : BaseApiController
{
    private readonly ICommandHandler<CreateBookCommand, BookResponse> _createBookHandler;
    private readonly IQueryHandler<GetBookByIdQuery, BookResponse> _getBookByIdHandler;
    private readonly IQueryHandler<GetBooksQuery, PagedResponse<BookResponse>> _getBooksHandler;
    private readonly ICommandHandler<UpdateBookCommand, BookResponse> _updateBookHandler;
    private readonly ICommandHandler<DeleteBookCommand> _deleteBookHandler;

    public BooksController(
        ICommandHandler<CreateBookCommand, BookResponse> createBookHandler,
        IQueryHandler<GetBookByIdQuery, BookResponse> getBookByIdHandler,
        IQueryHandler<GetBooksQuery, PagedResponse<BookResponse>> getBooksHandler,
        ICommandHandler<UpdateBookCommand, BookResponse> updateBookHandler,
        ICommandHandler<DeleteBookCommand> deleteBookHandler)
    {
        _createBookHandler = createBookHandler;
        _getBookByIdHandler = getBookByIdHandler;
        _getBooksHandler = getBooksHandler;
        _updateBookHandler = updateBookHandler;
        _deleteBookHandler = deleteBookHandler;
    }

    /// <summary>
    /// Returns a paged list of books with optional search and filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<BookResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<BookResponse>>> GetBooks(
        [FromQuery] GetBooksRequestQuery request,
        CancellationToken cancellationToken)
    {
        var response = await _getBooksHandler.Handle(new GetBooksQuery(request.ToFilter()), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Returns a single book by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BookResponse>> GetBookById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _getBookByIdHandler.Handle(new GetBookByIdQuery(id), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Creates a new book catalog record.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookResponse>> CreateBook(
        [FromBody] CreateBookRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _createBookHandler.Handle(new CreateBookCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetBookById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Updates an existing book.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookResponse>> UpdateBook(
        Guid id,
        [FromBody] UpdateBookRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _updateBookHandler.Handle(new UpdateBookCommand(id, request), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Deletes a book by identifier.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteBook(Guid id, CancellationToken cancellationToken)
    {
        await _deleteBookHandler.Handle(new DeleteBookCommand(id), cancellationToken);
        return NoContent();
    }
}
