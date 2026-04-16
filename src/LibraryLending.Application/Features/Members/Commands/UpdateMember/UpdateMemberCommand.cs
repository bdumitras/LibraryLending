using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Features.Members.Contracts;

namespace LibraryLending.Application.Features.Members.Commands.UpdateMember;

public sealed record UpdateMemberCommand(Guid Id, UpdateMemberRequest Request) : ICommand<MemberResponse>;
