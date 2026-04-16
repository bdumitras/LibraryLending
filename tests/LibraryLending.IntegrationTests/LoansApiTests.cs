using System.Net;
using System.Net.Http.Json;
using LibraryLending.Application.Common.Contracts;
using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Loans.Contracts;
using LibraryLending.Domain.Enums;
using LibraryLending.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LibraryLending.IntegrationTests;

[Collection(IntegrationTestCollectionNames.Name)]
public sealed class LoansApiTests : IntegrationTestBase
{
    public LoansApiTests(IntegrationTestEnvironment environment)
        : base(environment)
    {
    }

    [Fact]
    public async Task Post_loans_should_create_loan_and_return_created()
    {
        var scenario = await SeedBasicScenarioAsync();
        var loanDateUtc = DateTime.UtcNow.AddDays(-1);
        var dueDateUtc = DateTime.UtcNow.AddDays(13);

        var request = new CreateLoanRequest
        {
            BookId = scenario.AvailableBookId,
            MemberId = scenario.ActiveMemberId,
            LoanDateUtc = loanDateUtc,
            DueDateUtc = dueDateUtc
        };

        var response = await Client.PostAsJsonAsync("/api/loans", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var payload = await EnsureSuccessAndReadAsJsonAsync<LoanResponse>(response);

        Assert.Equal(scenario.AvailableBookId, payload.BookId);
        Assert.Equal(scenario.ActiveMemberId, payload.MemberId);
        Assert.Equal(LoanStatus.Active, payload.Status);
        Assert.True(payload.IsActive);
        Assert.False(payload.IsReturned);
        Assert.False(payload.IsOverdue);
        Assert.Null(payload.ReturnedAtUtc);
        Assert.Equal($"/api/loans/{payload.Id}", response.Headers.Location!.AbsolutePath);

        var persisted = await Environment.ExecuteDbContextAsync(async dbContext =>
        {
            var loan = await dbContext.Loans.AsNoTracking().SingleOrDefaultAsync(item => item.Id == payload.Id);
            var book = await dbContext.Books.AsNoTracking().SingleAsync(item => item.Id == scenario.AvailableBookId);
            return new { Loan = loan, Book = book };
        });

        Assert.NotNull(persisted.Loan);
        Assert.Equal(3, persisted.Book.AvailableCopies);
    }

    [Fact]
    public async Task Get_loan_by_id_should_return_seeded_loan()
    {
        var scenario = await SeedBasicScenarioAsync();

        var response = await Client.GetAsync($"/api/loans/{scenario.ActiveLoanId}");

        response.EnsureSuccessStatusCode();
        var payload = await ReadAsJsonAsync<LoanResponse>(response);

        Assert.NotNull(payload);
        Assert.Equal(scenario.ActiveLoanId, payload!.Id);
        Assert.Equal(scenario.LoanedBookId, payload.BookId);
        Assert.Equal(scenario.ActiveMemberId, payload.MemberId);
        Assert.Equal(LoanStatus.Active, payload.Status);
        Assert.True(payload.IsActive);
    }

    [Fact]
    public async Task Get_loan_by_id_should_return_not_found_for_unknown_loan()
    {
        var missingId = Guid.NewGuid();

        var response = await Client.GetAsync($"/api/loans/{missingId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var payload = await response.ReadJsonAsync<ErrorResponse>();
        Assert.NotNull(payload);
        Assert.Equal(404, payload!.Status);
        Assert.Equal("LoanNotFoundException", payload.ErrorCode);
    }

    [Fact]
    public async Task Post_loans_should_return_conflict_when_book_has_no_available_copies()
    {
        var ids = await SeedAsync(async dbContext =>
        {
            var exhaustedBook = TestDataFactory.CreateBook(
                title: "Single Copy Book",
                author: "Inventory Author",
                isbn: "9781000000101",
                publicationYear: 2024,
                totalCopies: 1,
                availableCopies: 1);

            var firstMember = TestDataFactory.CreateMember(
                fullName: "First Member",
                email: "first.member@example.com",
                isActive: true);

            var secondMember = TestDataFactory.CreateMember(
                fullName: "Second Member",
                email: "second.member@example.com",
                isActive: true);

            var existingLoan = TestDataFactory.CreateActiveLoan(exhaustedBook, firstMember);

            await dbContext.Books.AddAsync(exhaustedBook);
            await dbContext.Members.AddRangeAsync([firstMember, secondMember]);
            await dbContext.Loans.AddAsync(existingLoan);

            return (BookId: exhaustedBook.Id, MemberId: secondMember.Id);
        });

        var request = new CreateLoanRequest
        {
            BookId = ids.BookId,
            MemberId = ids.MemberId,
            LoanDateUtc = DateTime.UtcNow,
            DueDateUtc = DateTime.UtcNow.AddDays(7)
        };

        var response = await Client.PostAsJsonAsync("/api/loans", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var payload = await response.ReadJsonAsync<ErrorResponse>();
        Assert.NotNull(payload);
        Assert.Equal(409, payload!.Status);
        Assert.Equal("BookHasNoAvailableCopiesException", payload.ErrorCode);
    }

    [Fact]
    public async Task Post_loans_should_return_conflict_when_member_is_inactive()
    {
        var scenario = await SeedBasicScenarioAsync();

        var request = new CreateLoanRequest
        {
            BookId = scenario.AvailableBookId,
            MemberId = scenario.InactiveMemberId,
            LoanDateUtc = DateTime.UtcNow,
            DueDateUtc = DateTime.UtcNow.AddDays(10)
        };

        var response = await Client.PostAsJsonAsync("/api/loans", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var payload = await response.ReadJsonAsync<ErrorResponse>();
        Assert.NotNull(payload);
        Assert.Equal(409, payload!.Status);
        Assert.Equal("InactiveMemberCannotBorrowException", payload.ErrorCode);
    }

    [Fact]
    public async Task Post_loans_return_should_mark_loan_as_returned_and_restore_inventory()
    {
        var scenario = await SeedBasicScenarioAsync();
        var returnedAtUtc = DateTime.UtcNow;

        var request = new ReturnLoanRequest
        {
            ReturnedAtUtc = returnedAtUtc
        };

        var response = await Client.PostAsJsonAsync($"/api/loans/{scenario.ActiveLoanId}/return", request);

        response.EnsureSuccessStatusCode();
        var payload = await ReadAsJsonAsync<LoanResponse>(response);

        Assert.NotNull(payload);
        Assert.Equal(scenario.ActiveLoanId, payload!.Id);
        Assert.Equal(LoanStatus.Returned, payload.Status);
        Assert.True(payload.IsReturned);
        Assert.False(payload.IsActive);
        Assert.False(payload.IsOverdue);
        Assert.NotNull(payload.ReturnedAtUtc);
        Assert.InRange(payload.ReturnedAtUtc!.Value, returnedAtUtc.AddSeconds(-1), returnedAtUtc.AddSeconds(1));

        var persisted = await Environment.ExecuteDbContextAsync(async dbContext =>
        {
            var loan = await dbContext.Loans.AsNoTracking().SingleAsync(item => item.Id == scenario.ActiveLoanId);
            var book = await dbContext.Books.AsNoTracking().SingleAsync(item => item.Id == scenario.LoanedBookId);
            return new { Loan = loan, Book = book };
        });

        Assert.Equal(LoanStatus.Returned, persisted.Loan.Status);
        Assert.Equal(2, persisted.Book.AvailableCopies);
    }

    [Fact]
    public async Task Post_loans_return_should_return_conflict_when_loan_was_already_returned()
    {
        var scenario = await SeedBasicScenarioAsync();

        var request = new ReturnLoanRequest
        {
            ReturnedAtUtc = DateTime.UtcNow
        };

        var response = await Client.PostAsJsonAsync($"/api/loans/{scenario.ReturnedLoanId}/return", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var payload = await response.ReadJsonAsync<ErrorResponse>();
        Assert.NotNull(payload);
        Assert.Equal(409, payload!.Status);
        Assert.Equal("LoanAlreadyReturnedException", payload.ErrorCode);
    }

    [Fact]
    public async Task Get_loans_should_filter_by_active_returned_and_overdue_statuses()
    {
        await SeedAsync(async dbContext =>
        {
            await TestDataSeeder.SeedBasicScenarioAsync(dbContext);

            var overdueBook = TestDataFactory.CreateBook(
                title: "Overdue Book",
                author: "Scenario Author",
                isbn: "9781000000102",
                publicationYear: 2023,
                totalCopies: 1,
                availableCopies: 1);

            var overdueMember = TestDataFactory.CreateMember(
                fullName: "Overdue Member",
                email: "overdue.member@example.com",
                isActive: true);

            var overdueLoan = TestDataFactory.CreateOverdueLoan(overdueBook, overdueMember);

            await dbContext.Books.AddAsync(overdueBook);
            await dbContext.Members.AddAsync(overdueMember);
            await dbContext.Loans.AddAsync(overdueLoan);
        });

        var activeResponse = await Client.GetAsync("/api/loans?status=Active&page=1&pageSize=10");
        var returnedResponse = await Client.GetAsync("/api/loans?status=Returned&page=1&pageSize=10");
        var overdueResponse = await Client.GetAsync("/api/loans?status=Overdue&page=1&pageSize=10&sortBy=dueDate&sortDirection=Asc");

        var activePayload = await EnsureSuccessAndReadAsJsonAsync<PagedResponse<LoanResponse>>(activeResponse);
        var returnedPayload = await EnsureSuccessAndReadAsJsonAsync<PagedResponse<LoanResponse>>(returnedResponse);
        var overduePayload = await EnsureSuccessAndReadAsJsonAsync<PagedResponse<LoanResponse>>(overdueResponse);

        Assert.Single(activePayload.Items);
        Assert.All(activePayload.Items, loan => Assert.Equal(LoanStatus.Active, loan.Status));

        Assert.Single(returnedPayload.Items);
        Assert.All(returnedPayload.Items, loan => Assert.Equal(LoanStatus.Returned, loan.Status));

        Assert.Single(overduePayload.Items);
        Assert.All(overduePayload.Items, loan => Assert.Equal(LoanStatus.Overdue, loan.Status));
        Assert.All(overduePayload.Items, loan => Assert.True(loan.IsOverdue));
    }
}
