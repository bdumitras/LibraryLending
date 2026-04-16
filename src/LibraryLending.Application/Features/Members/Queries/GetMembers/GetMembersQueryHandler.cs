using FluentValidation;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Extensions;
using LibraryLending.Application.Common.Logging;
using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Members.Mappings;
using Microsoft.Extensions.Logging;

namespace LibraryLending.Application.Features.Members.Queries.GetMembers;

public sealed class GetMembersQueryHandler : IQueryHandler<GetMembersQuery, PagedResponse<Contracts.MemberResponse>>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IValidator<GetMembersQuery> _validator;
    private readonly ILogger<GetMembersQueryHandler> _logger;

    public GetMembersQueryHandler(
        IMemberRepository memberRepository,
        IValidator<GetMembersQuery> validator,
        ILogger<GetMembersQueryHandler> logger)
    {
        _memberRepository = memberRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<PagedResponse<Contracts.MemberResponse>> Handle(GetMembersQuery query, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var pagedMembers = await _memberRepository.GetPagedAsync(query.Filter, cancellationToken);

        _logger.LogDebug(
            ApplicationLogEvents.MembersListed,
            "Retrieved members page {Page} with page size {PageSize}. TotalCount {TotalCount}. SearchTerm {SearchTerm}. IsActive {IsActive}. SortBy {SortBy}. SortDirection {SortDirection}.",
            pagedMembers.Page,
            pagedMembers.PageSize,
            pagedMembers.TotalCount,
            query.Filter.SearchTerm,
            query.Filter.IsActive,
            query.Filter.SortBy,
            query.Filter.SortDirection);

        return pagedMembers.Map(member => member.ToResponse());
    }
}
