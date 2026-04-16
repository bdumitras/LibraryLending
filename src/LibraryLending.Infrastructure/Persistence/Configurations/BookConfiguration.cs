using LibraryLending.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryLending.Infrastructure.Persistence.Configurations;

public sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("books");

        builder.HasKey(book => book.Id);

        builder.Property(book => book.Id)
            .ValueGeneratedNever();

        builder.Property(book => book.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(book => book.Author)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(book => book.Isbn)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(book => book.PublicationYear)
            .IsRequired();

        builder.Property(book => book.TotalCopies)
            .IsRequired();

        builder.Property(book => book.AvailableCopies)
            .IsRequired();

        builder.Property(book => book.CreatedAtUtc)
            .IsRequired();

        builder.Property(book => book.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(book => book.Isbn)
            .IsUnique();

        builder.HasIndex(book => book.Title);

        builder.HasIndex(book => book.Author);

        builder.HasCheckConstraint(
            "CK_Books_TotalCopies_Positive",
            "\"TotalCopies\" > 0");

        builder.HasCheckConstraint(
            "CK_Books_AvailableCopies_NonNegative",
            "\"AvailableCopies\" >= 0");

        builder.HasCheckConstraint(
            "CK_Books_AvailableCopies_NotGreaterThan_TotalCopies",
            "\"AvailableCopies\" <= \"TotalCopies\"");
    }
}
