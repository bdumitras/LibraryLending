using FluentValidation;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Extensions;
using LibraryLending.Application.Common.Logging;
using LibraryLending.Application.Features.Members.Exceptions;
using LibraryLending.Application.Features.Members.Mappings;
using Microsoft.Extensions.Logging;

namespace LibraryLending.Application.Features.Members.Queries.GetMemberById;

public sealed class GetMemberByIdQueryHandler : IQueryHandler<GetMemberByIdQuery, Contracts.MemberResponse>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IValidator<GetMemberByIdQuery> _validator;
    private readonly ILogger<GetMemberByIdQueryHandler> _logger;

    public GetMemberByIdQueryHandler(
        IMemberRepository memberRepository,
        IValidator<GetMemberByIdQuery> validator,
        ILogger<GetMemberByIdQueryHandler> logger)
    {
        _memberRepository = memberRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Contracts.MemberResponse> Handle(GetMemberByIdQuery query, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var member = await _memberRepository.GetByIdAsync(query.Id, cancellationToken);
        if (member is null)
        {
            throw new MemberNotFoundException(query.Id);
        }

        _logger.LogDebug(
            ApplicationLogEvents.MemberRetrieved,
            "Retrieved member {MemberId} with email {Email}.",
            member.Id,
            member.Email);

        return member.ToResponse();
    }
}
