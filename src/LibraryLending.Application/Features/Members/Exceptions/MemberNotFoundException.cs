using LibraryLending.Application.Common.Exceptions;

namespace LibraryLending.Application.Features.Members.Exceptions;

public sealed class MemberNotFoundException : NotFoundException
{
    public MemberNotFoundException(Guid memberId)
        : base($"Member with id '{memberId}' was not found.")
    {
    }
}
