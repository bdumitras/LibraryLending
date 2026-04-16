using System.Net;
using System.Net.Http.Json;
using LibraryLending.Application.Common.Contracts;
using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Members.Contracts;
using LibraryLending.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LibraryLending.IntegrationTests;

[Collection(IntegrationTestCollectionNames.Name)]
public sealed class MembersApiTests : IntegrationTestBase
{
    public MembersApiTests(IntegrationTestEnvironment environment)
        : base(environment)
    {
    }

    [Fact]
    public async Task Post_members_should_create_member_and_return_created()
    {
        var request = new CreateMemberRequest
        {
            FullName = "Ada Lovelace",
            Email = "  ada.lovelace@example.com ",
            IsActive = true
        };

        var response = await Client.PostAsJsonAsync("/api/members", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var payload = await EnsureSuccessAndReadAsJsonAsync<MemberResponse>(response);

        Assert.Equal("Ada Lovelace", payload.FullName);
        Assert.Equal("ada.lovelace@example.com", payload.Email);
        Assert.True(payload.IsActive);
        Assert.Equal($"/api/members/{payload.Id}", response.Headers.Location!.AbsolutePath);

        var persisted = await Environment.ExecuteDbContextAsync(dbContext =>
            dbContext.Members.AsNoTracking().SingleOrDefaultAsync(member => member.Id == payload.Id));

        Assert.NotNull(persisted);
        Assert.Equal("Ada Lovelace", persisted!.FullName);
        Assert.Equal("ada.lovelace@example.com", persisted.Email);
        Assert.True(persisted.IsActive);
    }

    [Fact]
    public async Task Get_member_by_id_should_return_seeded_member()
    {
        var memberId = await SeedAsync(async dbContext =>
        {
            var member = TestDataFactory.CreateMember(
                fullName: "Grace Hopper",
                email: "grace.hopper@example.com",
                isActive: true);

            await dbContext.Members.AddAsync(member);
            return member.Id;
        });

        var response = await Client.GetAsync($"/api/members/{memberId}");

        response.EnsureSuccessStatusCode();
        var payload = await ReadAsJsonAsync<MemberResponse>(response);

        Assert.NotNull(payload);
        Assert.Equal(memberId, payload!.Id);
        Assert.Equal("Grace Hopper", payload.FullName);
        Assert.Equal("grace.hopper@example.com", payload.Email);
        Assert.True(payload.IsActive);
    }

    [Fact]
    public async Task Get_member_by_id_should_return_not_found_for_unknown_member()
    {
        var missingId = Guid.NewGuid();

        var response = await Client.GetAsync($"/api/members/{missingId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var payload = await response.ReadJsonAsync<ErrorResponse>();
        Assert.NotNull(payload);
        Assert.Equal(404, payload!.Status);
        Assert.Equal("MemberNotFoundException", payload.ErrorCode);
    }

    [Fact]
    public async Task Put_member_should_update_existing_member()
    {
        var memberId = await SeedAsync(async dbContext =>
        {
            var member = TestDataFactory.CreateMember(
                fullName: "Original Member",
                email: "original.member@example.com",
                isActive: true);

            await dbContext.Members.AddAsync(member);
            return member.Id;
        });

        var request = new UpdateMemberRequest
        {
            FullName = "Updated Member",
            Email = "updated.member@example.com",
            IsActive = false
        };

        var response = await Client.PutAsJsonAsync($"/api/members/{memberId}", request);

        response.EnsureSuccessStatusCode();
        var payload = await ReadAsJsonAsync<MemberResponse>(response);

        Assert.NotNull(payload);
        Assert.Equal(memberId, payload!.Id);
        Assert.Equal("Updated Member", payload.FullName);
        Assert.Equal("updated.member@example.com", payload.Email);
        Assert.False(payload.IsActive);

        var persisted = await Environment.ExecuteDbContextAsync(dbContext =>
            dbContext.Members.AsNoTracking().SingleAsync(member => member.Id == memberId));

        Assert.Equal("Updated Member", persisted.FullName);
        Assert.Equal("updated.member@example.com", persisted.Email);
        Assert.False(persisted.IsActive);
        Assert.NotNull(persisted.UpdatedAtUtc);
    }

    [Fact]
    public async Task Put_member_should_return_conflict_when_email_belongs_to_another_member()
    {
        var ids = await SeedAsync(async dbContext =>
        {
            var originalMember = TestDataFactory.CreateMember(
                fullName: "Original Member",
                email: "original.member@example.com",
                isActive: true);

            var conflictingMember = TestDataFactory.CreateMember(
                fullName: "Conflicting Member",
                email: "conflicting.member@example.com",
                isActive: true);

            await dbContext.Members.AddRangeAsync([originalMember, conflictingMember]);
            return (originalMember.Id, conflictingMember.Email);
        });

        var request = new UpdateMemberRequest
        {
            FullName = "Updated Original Member",
            Email = ids.Email,
            IsActive = true
        };

        var response = await Client.PutAsJsonAsync($"/api/members/{ids.Id}", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var payload = await response.ReadJsonAsync<ErrorResponse>();
        Assert.NotNull(payload);
        Assert.Equal(409, payload!.Status);
        Assert.Equal("MemberEmailAlreadyExistsException", payload.ErrorCode);
    }

    [Fact]
    public async Task Delete_member_should_remove_existing_member()
    {
        var memberId = await SeedAsync(async dbContext =>
        {
            var member = TestDataFactory.CreateMember(
                fullName: "Delete Me",
                email: "delete.me@example.com",
                isActive: true);

            await dbContext.Members.AddAsync(member);
            return member.Id;
        });

        var deleteResponse = await Client.DeleteAsync($"/api/members/{memberId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await Client.GetAsync($"/api/members/{memberId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var error = await getResponse.ReadJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("MemberNotFoundException", error!.ErrorCode);

        var exists = await Environment.ExecuteDbContextAsync(dbContext =>
            dbContext.Members.AsNoTracking().AnyAsync(member => member.Id == memberId));

        Assert.False(exists);
    }

    [Fact]
    public async Task Post_members_should_return_conflict_when_email_already_exists()
    {
        await SeedAsync(async dbContext =>
        {
            var existingMember = TestDataFactory.CreateMember(
                fullName: "Existing Member",
                email: "existing.member@example.com",
                isActive: true);

            await dbContext.Members.AddAsync(existingMember);
        });

        var request = new CreateMemberRequest
        {
            FullName = "Another Member",
            Email = "existing.member@example.com",
            IsActive = false
        };

        var response = await Client.PostAsJsonAsync("/api/members", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var payload = await response.ReadJsonAsync<ErrorResponse>();
        Assert.NotNull(payload);
        Assert.Equal(409, payload!.Status);
        Assert.Equal("MemberEmailAlreadyExistsException", payload.ErrorCode);
        Assert.Equal("Conflict", payload.Title);
    }

    [Fact]
    public async Task Get_members_should_support_search_filter_sort_and_paging()
    {
        await SeedAsync(async dbContext =>
        {
            var members = new[]
            {
                TestDataFactory.CreateMember(
                    fullName: "Alice Johnson",
                    email: "alice.johnson@example.com",
                    isActive: true),
                TestDataFactory.CreateMember(
                    fullName: "Alice Brown",
                    email: "alice.brown@example.com",
                    isActive: false),
                TestDataFactory.CreateMember(
                    fullName: "Bob Stone",
                    email: "bob.stone@example.com",
                    isActive: true),
                TestDataFactory.CreateMember(
                    fullName: "Charlie Brown",
                    email: "charlie.brown@example.com",
                    isActive: false),
                TestDataFactory.CreateMember(
                    fullName: "Diana Prince",
                    email: "diana.prince@example.com",
                    isActive: true)
            };

            await dbContext.Members.AddRangeAsync(members);
        });

        var response = await Client.GetAsync(
            "/api/members?page=1&pageSize=1&searchTerm=alice&email=example.com&isActive=false&sortBy=fullname&sortDirection=asc");

        response.EnsureSuccessStatusCode();
        var payload = await ReadAsJsonAsync<PagedResponse<MemberResponse>>(response);

        Assert.NotNull(payload);
        Assert.Equal(1, payload!.Page);
        Assert.Equal(1, payload.PageSize);
        Assert.Equal(1, payload.TotalCount);
        Assert.Equal(1, payload.TotalPages);
        Assert.False(payload.HasPreviousPage);
        Assert.False(payload.HasNextPage);

        var item = Assert.Single(payload.Items);
        Assert.Equal("Alice Brown", item.FullName);
        Assert.Equal("alice.brown@example.com", item.Email);
        Assert.False(item.IsActive);
    }
}
