using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Members.Models;
using LibraryLending.Domain.Entities;

namespace LibraryLending.Application.Common.Abstractions.Persistence;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Member?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, Guid? excludingId = null, CancellationToken cancellationToken = default);

    Task<PagedResponse<Member>> GetPagedAsync(MemberListFilter filter, CancellationToken cancellationToken = default);

    Task AddAsync(Member member, CancellationToken cancellationToken = default);

    void Update(Member member);

    void Remove(Member member);
}
