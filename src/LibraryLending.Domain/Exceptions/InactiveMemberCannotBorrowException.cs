namespace LibraryLending.Domain.Exceptions;

public sealed class InactiveMemberCannotBorrowException : DomainException
{
    public InactiveMemberCannotBorrowException(Guid memberId, string email)
        : base($"Member '{email}' ({memberId}) is inactive and cannot borrow books.")
    {
        MemberId = memberId;
        Email = email;
    }

    public Guid MemberId { get; }

    public string Email { get; }
}
