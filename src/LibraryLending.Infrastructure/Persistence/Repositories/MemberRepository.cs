using LibraryLending.Application.Common.Abstractions.Persistence;
using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Members.Models;
using LibraryLending.Domain.Entities;
using LibraryLending.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LibraryLending.Infrastructure.Persistence.Repositories;

public sealed class MemberRepository : IMemberRepository
{
    private readonly ApplicationDbContext _dbContext;

    public MemberRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Members.FirstOrDefaultAsync(member => member.Id == id, cancellationToken);
    }

    public Task<Member?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim();
        return _dbContext.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(member => member.Email == normalizedEmail, cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, Guid? excludingId = null, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim();

        return _dbContext.Members.AnyAsync(
            member => member.Email == normalizedEmail && (!excludingId.HasValue || member.Id != excludingId.Value),
            cancellationToken);
    }

    public Task<PagedResponse<Member>> GetPagedAsync(MemberListFilter filter, CancellationToken cancellationToken = default)
    {
        IQueryable<Member> query = _dbContext.Members.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var pattern = $"%{filter.SearchTerm.Trim()}%";
            query = query.Where(member =>
                EF.Functions.ILike(member.FullName, pattern) ||
                EF.Functions.ILike(member.Email, pattern));
        }

        if (!string.IsNullOrWhiteSpace(filter.FullName))
        {
            var pattern = $"%{filter.FullName.Trim()}%";
            query = query.Where(member => EF.Functions.ILike(member.FullName, pattern));
        }

        if (!string.IsNullOrWhiteSpace(filter.Email))
        {
            var pattern = $"%{filter.Email.Trim()}%";
            query = query.Where(member => EF.Functions.ILike(member.Email, pattern));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(member => member.IsActive == filter.IsActive.Value);
        }

        query = ApplyOrdering(query, filter);

        return query.ToPagedResponseAsync(filter.Page, filter.PageSize, cancellationToken);
    }

    public Task AddAsync(Member member, CancellationToken cancellationToken = default)
    {
        return _dbContext.Members.AddAsync(member, cancellationToken).AsTask();
    }

    public void Update(Member member)
    {
        _dbContext.Members.Update(member);
    }

    public void Remove(Member member)
    {
        _dbContext.Members.Remove(member);
    }

    private static IQueryable<Member> ApplyOrdering(IQueryable<Member> query, MemberListFilter filter)
    {
        var sortBy = filter.SortBy?.Trim().ToLowerInvariant();
        var descending = filter.SortDirection == SortDirection.Desc;

        return (sortBy, descending) switch
        {
            ("fullname", true) => query.OrderByDescending(member => member.FullName).ThenByDescending(member => member.Email).ThenBy(member => member.Id),
            ("fullname", false) => query.OrderBy(member => member.FullName).ThenBy(member => member.Email).ThenBy(member => member.Id),

            ("email", true) => query.OrderByDescending(member => member.Email).ThenByDescending(member => member.FullName).ThenBy(member => member.Id),
            ("email", false) => query.OrderBy(member => member.Email).ThenBy(member => member.FullName).ThenBy(member => member.Id),

            ("createdat", true) => query.OrderByDescending(member => member.CreatedAtUtc).ThenBy(member => member.FullName).ThenBy(member => member.Id),
            ("createdat", false) => query.OrderBy(member => member.CreatedAtUtc).ThenBy(member => member.FullName).ThenBy(member => member.Id),

            ("updatedat", true) => query.OrderByDescending(member => member.UpdatedAtUtc).ThenByDescending(member => member.CreatedAtUtc).ThenBy(member => member.Id),
            ("updatedat", false) => query.OrderBy(member => member.UpdatedAtUtc).ThenBy(member => member.CreatedAtUtc).ThenBy(member => member.Id),

            (_, true) => query.OrderByDescending(member => member.FullName).ThenByDescending(member => member.Email).ThenBy(member => member.Id),
            _ => query.OrderBy(member => member.FullName).ThenBy(member => member.Email).ThenBy(member => member.Id)
        };
    }
}
