using LibraryLending.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryLending.Infrastructure.Persistence.Configurations;

public sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("members");

        builder.HasKey(member => member.Id);

        builder.Property(member => member.Id)
            .ValueGeneratedNever();

        builder.Property(member => member.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(member => member.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(member => member.IsActive)
            .IsRequired();

        builder.Property(member => member.CreatedAtUtc)
            .IsRequired();

        builder.Property(member => member.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(member => member.Email)
            .IsUnique();

        builder.HasIndex(member => member.FullName);
    }
}
