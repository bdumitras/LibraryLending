namespace LibraryLending.Domain.Entities;

using LibraryLending.Domain.Common;
using LibraryLending.Domain.Exceptions;

public class Member : AuditableEntity
{
    private Member()
    {
    }

    public Member(string fullName, string email, bool isActive = true)
    {
        ApplyDetails(fullName, email, isActive, touch: false);
    }

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public void UpdateDetails(string fullName, string email, bool isActive)
    {
        ApplyDetails(fullName, email, isActive, touch: true);
    }

    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Touch();
    }

    public void EnsureCanBorrow()
    {
        if (!IsActive)
        {
            throw new InactiveMemberCannotBorrowException(Id, Email);
        }
    }

    private void ApplyDetails(string fullName, string email, bool isActive, bool touch)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainValidationException("Member full name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainValidationException("Member email is required.");
        }

        FullName = fullName.Trim();
        Email = email.Trim();
        IsActive = isActive;

        if (touch)
        {
            Touch();
        }
    }
}
