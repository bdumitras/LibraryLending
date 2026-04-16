namespace LibraryLending.UnitTests.Application.Validators.Books;

using LibraryLending.Application.Features.Books.Commands.CreateBook;
using LibraryLending.Application.Features.Books.Contracts;

public class CreateBookCommandValidatorTests
{
    private readonly CreateBookCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_Command_Is_Valid()
    {
        var command = new CreateBookCommand(new CreateBookRequest
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            Isbn = "9780132350884",
            PublicationYear = 2008,
            TotalCopies = 3,
            AvailableCopies = 2
        });

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Should_Fail_When_Request_Is_Null()
    {
        var command = new CreateBookCommand(null!);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Request");
    }

    [Fact]
    public void Validate_Should_Fail_When_Title_Is_Empty()
    {
        var command = new CreateBookCommand(new CreateBookRequest
        {
            Title = string.Empty,
            Author = "Robert C. Martin",
            Isbn = "9780132350884",
            PublicationYear = 2008,
            TotalCopies = 3,
            AvailableCopies = 2
        });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Request.Title");
    }

    [Fact]
    public void Validate_Should_Fail_When_PublicationYear_Is_Out_Of_Range()
    {
        var command = new CreateBookCommand(new CreateBookRequest
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            Isbn = "9780132350884",
            PublicationYear = 1200,
            TotalCopies = 3,
            AvailableCopies = 2
        });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Request.PublicationYear");
    }

    [Fact]
    public void Validate_Should_Fail_When_AvailableCopies_Exceed_TotalCopies()
    {
        var command = new CreateBookCommand(new CreateBookRequest
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            Isbn = "9780132350884",
            PublicationYear = 2008,
            TotalCopies = 2,
            AvailableCopies = 3
        });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Available copies cannot exceed total copies.");
    }
}
