using LibraryLending.Application.Features.Members.Contracts;
using LibraryLending.Domain.Entities;

namespace LibraryLending.Application.Features.Members.Mappings;

public static class MemberMappings
{
    public static MemberResponse ToResponse(this Member member)
    {
        ArgumentNullException.ThrowIfNull(member);

        return new MemberResponse
        {
            Id = member.Id,
            FullName = member.FullName,
            Email = member.Email,
            IsActive = member.IsActive,
            CreatedAtUtc = member.CreatedAtUtc,
            UpdatedAtUtc = member.UpdatedAtUtc
        };
    }
}
