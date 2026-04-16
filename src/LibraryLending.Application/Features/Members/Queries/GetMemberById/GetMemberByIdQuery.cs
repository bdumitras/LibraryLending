using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Features.Members.Contracts;

namespace LibraryLending.Application.Features.Members.Queries.GetMemberById;

public sealed record GetMemberByIdQuery(Guid Id) : IQuery<MemberResponse>;
