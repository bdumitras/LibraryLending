namespace LibraryLending.UnitTests.Domain.Entities;

using LibraryLending.Domain.Entities;
using LibraryLending.Domain.Exceptions;

public class BookTests
{
    [Fact]
    public void Constructor_Should_Set_Properties_When_Data_Is_Valid()
    {
        var book = new Book("Clean Code", "Robert C. Martin", "9780132350884", 2008, 3, 3);

        Assert.Equal("Clean Code", book.Title);
        Assert.Equal("Robert C. Martin", book.Author);
        Assert.Equal("9780132350884", book.Isbn);
        Assert.Equal(2008, book.PublicationYear);
        Assert.Equal(3, book.TotalCopies);
        Assert.Equal(3, book.AvailableCopies);
        Assert.True(book.HasAvailableCopies);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Title_Is_Empty()
    {
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Book("  ", "Robert C. Martin", "9780132350884", 2008, 3, 3));

        Assert.Equal("Book title is required.", ex.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_When_AvailableCopies_Exceed_TotalCopies()
    {
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Book("Clean Code", "Robert C. Martin", "9780132350884", 2008, 2, 3));

        Assert.Contains("cannot exceed total copies", ex.Message);
    }

    [Fact]
    public void BorrowCopy_Should_Decrement_AvailableCopies_When_Book_Is_Available()
    {
        var book = new Book("Clean Code", "Robert C. Martin", "9780132350884", 2008, 2, 1);

        book.BorrowCopy();

        Assert.Equal(0, book.AvailableCopies);
        Assert.False(book.HasAvailableCopies);
    }

    [Fact]
    public void BorrowCopy_Should_Throw_When_No_Copies_Are_Available()
    {
        var book = new Book("Clean Code", "Robert C. Martin", "9780132350884", 2008, 1, 0);

        Assert.Throws<BookHasNoAvailableCopiesException>(() => book.BorrowCopy());
    }

    [Fact]
    public void ReturnCopy_Should_Increment_AvailableCopies_When_Below_TotalCopies()
    {
        var book = new Book("Clean Code", "Robert C. Martin", "9780132350884", 2008, 3, 2);

        book.ReturnCopy();

        Assert.Equal(3, book.AvailableCopies);
    }

    [Fact]
    public void ReturnCopy_Should_Throw_When_Inventory_Would_Overflow()
    {
        var book = new Book("Clean Code", "Robert C. Martin", "9780132350884", 2008, 3, 3);

        Assert.Throws<BookInventoryOverflowException>(() => book.ReturnCopy());
    }
}
