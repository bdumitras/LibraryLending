using LibraryLending.Application.Common.Exceptions;

namespace LibraryLending.Application.Features.Members.Exceptions;

public sealed class MemberEmailAlreadyExistsException : ConflictException
{
    public MemberEmailAlreadyExistsException(string email)
        : base($"A member with email '{email}' already exists.")
    {
    }
}
