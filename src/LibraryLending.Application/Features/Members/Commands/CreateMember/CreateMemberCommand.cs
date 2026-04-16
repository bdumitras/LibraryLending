using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Application.Features.Members.Contracts;

namespace LibraryLending.Application.Features.Members.Commands.CreateMember;

public sealed record CreateMemberCommand(CreateMemberRequest Request) : ICommand<MemberResponse>;
