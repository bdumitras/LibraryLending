using LibraryLending.Application.Common.Abstractions.Messaging;

namespace LibraryLending.Application.Features.Members.Commands.DeleteMember;

public sealed record DeleteMemberCommand(Guid Id) : ICommand;
