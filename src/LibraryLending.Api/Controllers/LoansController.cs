using LibraryLending.Api.Models.Loans;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Contracts;
using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Loans.Commands.CreateLoan;
using LibraryLending.Application.Features.Loans.Commands.ReturnLoan;
using LibraryLending.Application.Features.Loans.Contracts;
using LibraryLending.Application.Features.Loans.Queries.GetLoanById;
using LibraryLending.Application.Features.Loans.Queries.GetLoans;
using Microsoft.AspNetCore.Mvc;

namespace LibraryLending.Api.Controllers;

/// <summary>
/// Exposes endpoints for creating, returning, and querying library loans.
/// </summary>
public sealed class LoansController : BaseApiController
{
    private readonly ICommandHandler<CreateLoanCommand, LoanResponse> _createLoanHandler;
    private readonly ICommandHandler<ReturnLoanCommand, LoanResponse> _returnLoanHandler;
    private readonly IQueryHandler<GetLoanByIdQuery, LoanResponse> _getLoanByIdHandler;
    private readonly IQueryHandler<GetLoansQuery, PagedResponse<LoanResponse>> _getLoansHandler;

    public LoansController(
        ICommandHandler<CreateLoanCommand, LoanResponse> createLoanHandler,
        ICommandHandler<ReturnLoanCommand, LoanResponse> returnLoanHandler,
        IQueryHandler<GetLoanByIdQuery, LoanResponse> getLoanByIdHandler,
        IQueryHandler<GetLoansQuery, PagedResponse<LoanResponse>> getLoansHandler)
    {
        _createLoanHandler = createLoanHandler;
        _returnLoanHandler = returnLoanHandler;
        _getLoanByIdHandler = getLoanByIdHandler;
        _getLoansHandler = getLoansHandler;
    }

    /// <summary>
    /// Returns a paged list of loans with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<LoanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<LoanResponse>>> GetLoans(
        [FromQuery] GetLoansRequestQuery request,
        CancellationToken cancellationToken)
    {
        var response = await _getLoansHandler.Handle(new GetLoansQuery(request.ToFilter()), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Returns a single loan by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanResponse>> GetLoanById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _getLoanByIdHandler.Handle(new GetLoanByIdQuery(id), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Creates a new loan for a book and member.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoanResponse>> CreateLoan(
        [FromBody] CreateLoanRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _createLoanHandler.Handle(new CreateLoanCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetLoanById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Marks an existing loan as returned.
    /// </summary>
    [HttpPost("{id:guid}/return")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoanResponse>> ReturnLoan(
        Guid id,
        [FromBody] ReturnLoanRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _returnLoanHandler.Handle(new ReturnLoanCommand(id, request), cancellationToken);
        return Ok(response);
    }
}
