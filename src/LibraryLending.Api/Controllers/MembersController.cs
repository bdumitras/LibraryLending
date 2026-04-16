using LibraryLending.Api.Models.Members;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Contracts;
using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Members.Commands.CreateMember;
using LibraryLending.Application.Features.Members.Commands.DeleteMember;
using LibraryLending.Application.Features.Members.Commands.UpdateMember;
using LibraryLending.Application.Features.Members.Contracts;
using LibraryLending.Application.Features.Members.Queries.GetMemberById;
using LibraryLending.Application.Features.Members.Queries.GetMembers;
using Microsoft.AspNetCore.Mvc;

namespace LibraryLending.Api.Controllers;

/// <summary>
/// Exposes CRUD endpoints for library members.
/// </summary>
public sealed class MembersController : BaseApiController
{
    private readonly ICommandHandler<CreateMemberCommand, MemberResponse> _createMemberHandler;
    private readonly IQueryHandler<GetMemberByIdQuery, MemberResponse> _getMemberByIdHandler;
    private readonly IQueryHandler<GetMembersQuery, PagedResponse<MemberResponse>> _getMembersHandler;
    private readonly ICommandHandler<UpdateMemberCommand, MemberResponse> _updateMemberHandler;
    private readonly ICommandHandler<DeleteMemberCommand> _deleteMemberHandler;

    public MembersController(
        ICommandHandler<CreateMemberCommand, MemberResponse> createMemberHandler,
        IQueryHandler<GetMemberByIdQuery, MemberResponse> getMemberByIdHandler,
        IQueryHandler<GetMembersQuery, PagedResponse<MemberResponse>> getMembersHandler,
        ICommandHandler<UpdateMemberCommand, MemberResponse> updateMemberHandler,
        ICommandHandler<DeleteMemberCommand> deleteMemberHandler)
    {
        _createMemberHandler = createMemberHandler;
        _getMemberByIdHandler = getMemberByIdHandler;
        _getMembersHandler = getMembersHandler;
        _updateMemberHandler = updateMemberHandler;
        _deleteMemberHandler = deleteMemberHandler;
    }

    /// <summary>
    /// Returns a paged list of members with optional search and filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<MemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<MemberResponse>>> GetMembers(
        [FromQuery] GetMembersRequestQuery request,
        CancellationToken cancellationToken)
    {
        var response = await _getMembersHandler.Handle(new GetMembersQuery(request.ToFilter()), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Returns a single member by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MemberResponse>> GetMemberById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _getMemberByIdHandler.Handle(new GetMemberByIdQuery(id), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Creates a new library member.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MemberResponse>> CreateMember(
        [FromBody] CreateMemberRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _createMemberHandler.Handle(new CreateMemberCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetMemberById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Updates an existing member.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MemberResponse>> UpdateMember(
        Guid id,
        [FromBody] UpdateMemberRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _updateMemberHandler.Handle(new UpdateMemberCommand(id, request), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Deletes a member by identifier.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteMember(Guid id, CancellationToken cancellationToken)
    {
        await _deleteMemberHandler.Handle(new DeleteMemberCommand(id), cancellationToken);
        return NoContent();
    }
}
