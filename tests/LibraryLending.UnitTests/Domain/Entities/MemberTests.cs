namespace LibraryLending.UnitTests.Domain.Entities;

using LibraryLending.Domain.Entities;
using LibraryLending.Domain.Exceptions;

public class MemberTests
{
    [Fact]
    public void Constructor_Should_Set_Properties_When_Data_Is_Valid()
    {
        var member = new Member("Jane Doe", "jane@example.com", true);

        Assert.Equal("Jane Doe", member.FullName);
        Assert.Equal("jane@example.com", member.Email);
        Assert.True(member.IsActive);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Email_Is_Empty()
    {
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Member("Jane Doe", "   ", true));

        Assert.Equal("Member email is required.", ex.Message);
    }

    [Fact]
    public void EnsureCanBorrow_Should_Throw_When_Member_Is_Inactive()
    {
        var member = new Member("Jane Doe", "jane@example.com", false);

        Assert.Throws<InactiveMemberCannotBorrowException>(() => member.EnsureCanBorrow());
    }

    [Fact]
    public void Activate_Should_Set_IsActive_To_True()
    {
        var member = new Member("Jane Doe", "jane@example.com", false);

        member.Activate();

        Assert.True(member.IsActive);
    }

    [Fact]
    public void Deactivate_Should_Set_IsActive_To_False()
    {
        var member = new Member("Jane Doe", "jane@example.com", true);

        member.Deactivate();

        Assert.False(member.IsActive);
    }
}
