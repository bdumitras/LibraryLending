using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Members.Contracts;
using LibraryLending.Application.Features.Members.Models;

namespace LibraryLending.Application.Features.Members.Queries.GetMembers;

public sealed record GetMembersQuery(MemberListFilter Filter) : IQuery<PagedResponse<MemberResponse>>;
