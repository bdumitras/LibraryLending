namespace LibraryLending.UnitTests.Application.Validators.Members;

using LibraryLending.Application.Features.Members.Commands.CreateMember;
using LibraryLending.Application.Features.Members.Contracts;

public class CreateMemberCommandValidatorTests
{
    private readonly CreateMemberCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_Command_Is_Valid()
    {
        var command = new CreateMemberCommand(new CreateMemberRequest
        {
            FullName = "Ada Lovelace",
            Email = "ada@example.com",
            IsActive = true
        });

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Should_Fail_When_Request_Is_Null()
    {
        var command = new CreateMemberCommand(null!);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Request");
    }

    [Fact]
    public void Validate_Should_Fail_When_FullName_Is_Empty()
    {
        var command = new CreateMemberCommand(new CreateMemberRequest
        {
            FullName = string.Empty,
            Email = "ada@example.com",
            IsActive = true
        });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Request.FullName");
    }

    [Fact]
    public void Validate_Should_Fail_When_Email_Is_Invalid()
    {
        var command = new CreateMemberCommand(new CreateMemberRequest
        {
            FullName = "Ada Lovelace",
            Email = "not-an-email",
            IsActive = true
        });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Request.Email");
    }

    [Fact]
    public void Validate_Should_Fail_When_Email_Is_Too_Long()
    {
        var email = $"{new string('a', 251)}@x.com";
        var command = new CreateMemberCommand(new CreateMemberRequest
        {
            FullName = "Ada Lovelace",
            Email = email,
            IsActive = true
        });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Request.Email");
    }
}
