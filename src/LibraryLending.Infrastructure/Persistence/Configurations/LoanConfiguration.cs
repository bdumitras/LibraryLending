using LibraryLending.Domain.Entities;
using LibraryLending.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryLending.Infrastructure.Persistence.Configurations;

public sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("loans");

        builder.HasKey(loan => loan.Id);

        builder.Property(loan => loan.Id)
            .ValueGeneratedNever();

        builder.Property(loan => loan.BookId)
            .IsRequired();

        builder.Property(loan => loan.MemberId)
            .IsRequired();

        builder.Property(loan => loan.LoanDateUtc)
            .IsRequired();

        builder.Property(loan => loan.DueDateUtc)
            .IsRequired();

        builder.Property(loan => loan.ReturnedAtUtc);

        builder.Property(loan => loan.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(loan => loan.CreatedAtUtc)
            .IsRequired();

        builder.Property(loan => loan.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(loan => loan.BookId);
        builder.HasIndex(loan => loan.MemberId);
        builder.HasIndex(loan => loan.Status);
        builder.HasIndex(loan => loan.DueDateUtc);
        builder.HasIndex(loan => new { loan.BookId, loan.Status });
        builder.HasIndex(loan => new { loan.MemberId, loan.Status });

        builder.HasOne<Book>()
            .WithMany()
            .HasForeignKey(loan => loan.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(loan => loan.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasCheckConstraint(
            "CK_Loans_DueDate_After_LoanDate",
            "\"DueDateUtc\" > \"LoanDateUtc\"");

        builder.HasCheckConstraint(
            "CK_Loans_ReturnedAt_After_Or_Equal_LoanDate",
            "\"ReturnedAtUtc\" IS NULL OR \"ReturnedAtUtc\" >= \"LoanDateUtc\"");

        builder.HasCheckConstraint(
            "CK_Loans_Status_Valid",
            string.Join(
                " OR ",
                Enum.GetNames<LoanStatus>().Select(name => $"\"Status\" = '{name}'")));
    }
}
