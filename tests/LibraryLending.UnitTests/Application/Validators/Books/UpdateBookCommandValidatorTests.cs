namespace LibraryLending.UnitTests.Application.Validators.Books;

using LibraryLending.Application.Features.Books.Commands.UpdateBook;
using LibraryLending.Application.Features.Books.Contracts;

public class UpdateBookCommandValidatorTests
{
    private readonly UpdateBookCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_Command_Is_Valid()
    {
        var command = new UpdateBookCommand(Guid.NewGuid(), new UpdateBookRequest
        {
            Title = "Domain-Driven Design",
            Author = "Eric Evans",
            Isbn = "9780321125217",
            PublicationYear = 2003,
            TotalCopies = 5,
            AvailableCopies = 4
        });

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Should_Fail_When_Id_Is_Empty()
    {
        var command = new UpdateBookCommand(Guid.Empty, new UpdateBookRequest
        {
            Title = "Domain-Driven Design",
            Author = "Eric Evans",
            Isbn = "9780321125217",
            PublicationYear = 2003,
            TotalCopies = 5,
            AvailableCopies = 4
        });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Id");
    }

    [Fact]
    public void Validate_Should_Fail_When_Request_Is_Null()
    {
        var command = new UpdateBookCommand(Guid.NewGuid(), null!);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Request");
    }

    [Fact]
    public void Validate_Should_Fail_When_Isbn_Is_Too_Long()
    {
        var command = new UpdateBookCommand(Guid.NewGuid(), new UpdateBookRequest
        {
            Title = "Domain-Driven Design",
            Author = "Eric Evans",
            Isbn = new string('9', 33),
            PublicationYear = 2003,
            TotalCopies = 5,
            AvailableCopies = 4
        });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Request.Isbn");
    }

    [Fact]
    public void Validate_Should_Fail_When_AvailableCopies_Exceed_TotalCopies()
    {
        var command = new UpdateBookCommand(Guid.NewGuid(), new UpdateBookRequest
        {
            Title = "Domain-Driven Design",
            Author = "Eric Evans",
            Isbn = "9780321125217",
            PublicationYear = 2003,
            TotalCopies = 2,
            AvailableCopies = 3
        });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Available copies cannot exceed total copies.");
    }
}
