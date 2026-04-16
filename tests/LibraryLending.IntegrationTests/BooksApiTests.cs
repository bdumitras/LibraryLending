using System.Net;
using System.Net.Http.Json;
using LibraryLending.Application.Common.Contracts;
using LibraryLending.Application.Common.Models;
using LibraryLending.Application.Features.Books.Contracts;
using LibraryLending.Domain.Entities;
using LibraryLending.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LibraryLending.IntegrationTests;

[Collection(IntegrationTestCollectionNames.Name)]
public sealed class BooksApiTests : IntegrationTestBase
{
    public BooksApiTests(IntegrationTestEnvironment environment)
        : base(environment)
    {
    }

    [Fact]
    public async Task Post_books_should_create_book_and_return_created()
    {
        var request = new CreateBookRequest
        {
            Title = "The Pragmatic Programmer",
            Author = "Andrew Hunt",
            Isbn = " 9780135957059 ",
            PublicationYear = 2019,
            TotalCopies = 5,
            AvailableCopies = 5
        };

        var response = await Client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var payload = await EnsureSuccessAndReadAsJsonAsync<BookResponse>(response);

        Assert.Equal("The Pragmatic Programmer", payload.Title);
        Assert.Equal("Andrew Hunt", payload.Author);
        Assert.Equal("9780135957059", payload.Isbn);
        Assert.Equal(2019, payload.PublicationYear);
        Assert.Equal(5, payload.TotalCopies);
        Assert.Equal(5, payload.AvailableCopies);
        Assert.True(payload.HasAvailableCopies);
        Assert.Equal($"/api/books/{payload.Id}", response.Headers.Location!.AbsolutePath);

        var persisted = await Environment.ExecuteDbContextAsync(dbContext =>
            dbContext.Books.AsNoTracking().SingleOrDefaultAsync(book => book.Id == payload.Id));

        Assert.NotNull(persisted);
        Assert.Equal("9780135957059", persisted!.Isbn);
    }

    [Fact]
    public async Task Get_book_by_id_should_return_seeded_book()
    {
        var bookId = await SeedAsync(async dbContext =>
        {
            var book = TestDataFactory.CreateBook(
                title: "Refactoring",
                author: "Martin Fowler",
                isbn: "9780201485677",
                publicationYear: 1999,
                totalCopies: 3,
                availableCopies: 2);

            await dbContext.Books.AddAsync(book);
            return book.Id;
        });

        var response = await Client.GetAsync($"/api/books/{bookId}");

        response.EnsureSuccessStatusCode();
        var payload = await ReadAsJsonAsync<BookResponse>(response);

        Assert.NotNull(payload);
        Assert.Equal(bookId, payload!.Id);
        Assert.Equal("Refactoring", payload.Title);
        Assert.Equal("Martin Fowler", payload.Author);
        Assert.Equal("9780201485677", payload.Isbn);
        Assert.Equal(2, payload.AvailableCopies);
    }


    [Fact]
    public async Task Get_book_by_id_should_return_not_found_for_unknown_book()
    {
        var missingId = Guid.NewGuid();

        var response = await Client.GetAsync($"/api/books/{missingId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var payload = await response.ReadJsonAsync<ErrorResponse>();
        Assert.NotNull(payload);
        Assert.Equal(404, payload!.Status);
        Assert.Equal("BookNotFoundException", payload.ErrorCode);
    }

    [Fact]
    public async Task Put_book_should_update_existing_book()
    {
        var bookId = await SeedAsync(async dbContext =>
        {
            var book = TestDataFactory.CreateBook(
                title: "Old Title",
                author: "Old Author",
                isbn: "9780131103627",
                publicationYear: 1988,
                totalCopies: 2,
                availableCopies: 1);

            await dbContext.Books.AddAsync(book);
            return book.Id;
        });

        var request = new UpdateBookRequest
        {
            Title = "Clean Architecture",
            Author = "Robert C. Martin",
            Isbn = "9780134494166",
            PublicationYear = 2017,
            TotalCopies = 6,
            AvailableCopies = 4
        };

        var response = await Client.PutAsJsonAsync($"/api/books/{bookId}", request);

        response.EnsureSuccessStatusCode();
        var payload = await ReadAsJsonAsync<BookResponse>(response);

        Assert.NotNull(payload);
        Assert.Equal(bookId, payload!.Id);
        Assert.Equal("Clean Architecture", payload.Title);
        Assert.Equal("Robert C. Martin", payload.Author);
        Assert.Equal("9780134494166", payload.Isbn);
        Assert.Equal(2017, payload.PublicationYear);
        Assert.Equal(6, payload.TotalCopies);
        Assert.Equal(4, payload.AvailableCopies);

        var persisted = await Environment.ExecuteDbContextAsync(dbContext =>
            dbContext.Books.AsNoTracking().SingleAsync(book => book.Id == bookId));

        Assert.Equal("Clean Architecture", persisted.Title);
        Assert.Equal("Robert C. Martin", persisted.Author);
        Assert.Equal("9780134494166", persisted.Isbn);
        Assert.Equal(6, persisted.TotalCopies);
        Assert.Equal(4, persisted.AvailableCopies);
        Assert.NotNull(persisted.UpdatedAtUtc);
    }


    [Fact]
    public async Task Put_book_should_return_conflict_when_isbn_belongs_to_another_book()
    {
        var ids = await SeedAsync(async dbContext =>
        {
            var originalBook = TestDataFactory.CreateBook(
                title: "Original Book",
                author: "Original Author",
                isbn: "9780137081073",
                publicationYear: 2010,
                totalCopies: 2,
                availableCopies: 2);

            var conflictingBook = TestDataFactory.CreateBook(
                title: "Conflicting Book",
                author: "Conflicting Author",
                isbn: "9780201633610",
                publicationYear: 1994,
                totalCopies: 3,
                availableCopies: 3);

            await dbContext.Books.AddRangeAsync([originalBook, conflictingBook]);
            return (originalBook.Id, conflictingBook.Isbn);
        });

        var request = new UpdateBookRequest
        {
            Title = "Updated Title",
            Author = "Updated Author",
            Isbn = ids.Isbn,
            PublicationYear = 2011,
            TotalCopies = 2,
            AvailableCopies = 2
        };

        var response = await Client.PutAsJsonAsync($"/api/books/{ids.Id}", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var payload = await response.ReadJsonAsync<ErrorResponse>();
        Assert.NotNull(payload);
        Assert.Equal(409, payload!.Status);
        Assert.Equal("BookIsbnAlreadyExistsException", payload.ErrorCode);
    }

    [Fact]
    public async Task Delete_book_should_remove_existing_book()
    {
        var bookId = await SeedAsync(async dbContext =>
        {
            var book = TestDataFactory.CreateBook(
                title: "Delete Me",
                author: "Cleanup Author",
                isbn: "9780132350884",
                publicationYear: 2008,
                totalCopies: 1,
                availableCopies: 1);

            await dbContext.Books.AddAsync(book);
            return book.Id;
        });

        var deleteResponse = await Client.DeleteAsync($"/api/books/{bookId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await Client.GetAsync($"/api/books/{bookId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var error = await getResponse.ReadJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("BookNotFoundException", error!.ErrorCode);

        var exists = await Environment.ExecuteDbContextAsync(dbContext =>
            dbContext.Books.AsNoTracking().AnyAsync(book => book.Id == bookId));

        Assert.False(exists);
    }

    [Fact]
    public async Task Post_books_should_return_conflict_when_isbn_already_exists()
    {
        await SeedAsync(async dbContext =>
        {
            var existingBook = TestDataFactory.CreateBook(
                title: "Existing Book",
                author: "Existing Author",
                isbn: "9780321125217",
                publicationYear: 2004,
                totalCopies: 2,
                availableCopies: 2);

            await dbContext.Books.AddAsync(existingBook);
        });

        var request = new CreateBookRequest
        {
            Title = "Another Book",
            Author = "Another Author",
            Isbn = "9780321125217",
            PublicationYear = 2005,
            TotalCopies = 3,
            AvailableCopies = 3
        };

        var response = await Client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var payload = await response.ReadJsonAsync<ErrorResponse>();
        Assert.NotNull(payload);
        Assert.Equal(409, payload!.Status);
        Assert.Equal("BookIsbnAlreadyExistsException", payload.ErrorCode);
        Assert.Equal("Conflict", payload.Title);
    }

    [Fact]
    public async Task Get_books_should_support_search_filter_sort_and_paging()
    {
        await SeedAsync(async dbContext =>
        {
            var books = new[]
            {
                TestDataFactory.CreateBook(
                    title: "Clean Architecture",
                    author: "Robert C. Martin",
                    isbn: "9780134494166",
                    publicationYear: 2017,
                    totalCopies: 5,
                    availableCopies: 5),
                TestDataFactory.CreateBook(
                    title: "Clean Code",
                    author: "Robert C. Martin",
                    isbn: "9780132350884",
                    publicationYear: 2008,
                    totalCopies: 4,
                    availableCopies: 1),
                TestDataFactory.CreateBook(
                    title: "Legacy Systems",
                    author: "Robert C. Martin",
                    isbn: "9780131177055",
                    publicationYear: 2004,
                    totalCopies: 2,
                    availableCopies: 0),
                TestDataFactory.CreateBook(
                    title: "Code Complete",
                    author: "Steve McConnell",
                    isbn: "9780735619678",
                    publicationYear: 2004,
                    totalCopies: 3,
                    availableCopies: 3),
                TestDataFactory.CreateBook(
                    title: "Domain-Driven Design",
                    author: "Eric Evans",
                    isbn: "9780321125217",
                    publicationYear: 2003,
                    totalCopies: 2,
                    availableCopies: 2)
            };

            await dbContext.Books.AddRangeAsync(books);
        });

        var response = await Client.GetAsync(
            "/api/books?page=1&pageSize=1&searchTerm=clean&author=Robert&publicationYearFrom=2008&publicationYearTo=2020&isAvailable=true&sortBy=publicationYear&sortDirection=desc");

        response.EnsureSuccessStatusCode();
        var payload = await ReadAsJsonAsync<PagedResponse<BookResponse>>(response);

        Assert.NotNull(payload);
        Assert.Equal(1, payload!.Page);
        Assert.Equal(1, payload.PageSize);
        Assert.Equal(2, payload.TotalCount);
        Assert.Equal(2, payload.TotalPages);
        Assert.False(payload.HasPreviousPage);
        Assert.True(payload.HasNextPage);

        var item = Assert.Single(payload.Items);
        Assert.Equal("Clean Architecture", item.Title);
        Assert.Equal("Robert C. Martin", item.Author);
        Assert.Equal(2017, item.PublicationYear);
        Assert.True(item.HasAvailableCopies);
    }
}
