using System;
using LibraryLending.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LibraryLending.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260414130000_InitialCreate")]
partial class InitialCreate
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "9.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("LibraryLending.Domain.Entities.Book", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedNever()
                    .HasColumnType("uuid");

                b.Property<int>("AvailableCopies")
                    .HasColumnType("integer");

                b.Property<string>("Author")
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnType("character varying(150)");

                b.Property<DateTime>("CreatedAtUtc")
                    .HasColumnType("timestamp with time zone");

                b.Property<string>("Isbn")
                    .IsRequired()
                    .HasMaxLength(32)
                    .HasColumnType("character varying(32)");

                b.Property<int>("PublicationYear")
                    .HasColumnType("integer");

                b.Property<string>("Title")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("character varying(200)");

                b.Property<int>("TotalCopies")
                    .HasColumnType("integer");

                b.Property<DateTime>("UpdatedAtUtc")
                    .HasColumnType("timestamp with time zone");

                b.HasKey("Id");

                b.HasIndex("Author");

                b.HasIndex("Isbn")
                    .IsUnique();

                b.HasIndex("Title");

                b.ToTable("books", t =>
                    {
                        t.HasCheckConstraint("CK_Books_AvailableCopies_NonNegative", "\"AvailableCopies\" >= 0");
                        t.HasCheckConstraint("CK_Books_AvailableCopies_NotGreaterThan_TotalCopies", "\"AvailableCopies\" <= \"TotalCopies\"");
                        t.HasCheckConstraint("CK_Books_TotalCopies_Positive", "\"TotalCopies\" > 0");
                    });
            });

        modelBuilder.Entity("LibraryLending.Domain.Entities.Member", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedNever()
                    .HasColumnType("uuid");

                b.Property<DateTime>("CreatedAtUtc")
                    .HasColumnType("timestamp with time zone");

                b.Property<string>("Email")
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnType("character varying(256)");

                b.Property<string>("FullName")
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnType("character varying(150)");

                b.Property<bool>("IsActive")
                    .HasColumnType("boolean");

                b.Property<DateTime>("UpdatedAtUtc")
                    .HasColumnType("timestamp with time zone");

                b.HasKey("Id");

                b.HasIndex("Email")
                    .IsUnique();

                b.HasIndex("FullName");

                b.ToTable("members");
            });

        modelBuilder.Entity("LibraryLending.Domain.Entities.Loan", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedNever()
                    .HasColumnType("uuid");

                b.Property<Guid>("BookId")
                    .HasColumnType("uuid");

                b.Property<DateTime>("CreatedAtUtc")
                    .HasColumnType("timestamp with time zone");

                b.Property<DateTime>("DueDateUtc")
                    .HasColumnType("timestamp with time zone");

                b.Property<DateTime>("LoanDateUtc")
                    .HasColumnType("timestamp with time zone");

                b.Property<Guid>("MemberId")
                    .HasColumnType("uuid");

                b.Property<DateTime?>("ReturnedAtUtc")
                    .HasColumnType("timestamp with time zone");

                b.Property<string>("Status")
                    .IsRequired()
                    .HasMaxLength(32)
                    .HasColumnType("character varying(32)");

                b.Property<DateTime>("UpdatedAtUtc")
                    .HasColumnType("timestamp with time zone");

                b.HasKey("Id");

                b.HasIndex("BookId");

                b.HasIndex("BookId", "Status");

                b.HasIndex("DueDateUtc");

                b.HasIndex("MemberId");

                b.HasIndex("MemberId", "Status");

                b.HasIndex("Status");

                b.ToTable("loans", t =>
                    {
                        t.HasCheckConstraint("CK_Loans_DueDate_After_LoanDate", "\"DueDateUtc\" > \"LoanDateUtc\"");
                        t.HasCheckConstraint("CK_Loans_ReturnedAt_After_Or_Equal_LoanDate", "\"ReturnedAtUtc\" IS NULL OR \"ReturnedAtUtc\" >= \"LoanDateUtc\"");
                        t.HasCheckConstraint("CK_Loans_Status_Valid", "\"Status\" = 'Active' OR \"Status\" = 'Returned' OR \"Status\" = 'Overdue'");
                    });
            });

        modelBuilder.Entity("LibraryLending.Domain.Entities.Loan", b =>
            {
                b.HasOne("LibraryLending.Domain.Entities.Book", null)
                    .WithMany()
                    .HasForeignKey("BookId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne("LibraryLending.Domain.Entities.Member", null)
                    .WithMany()
                    .HasForeignKey("MemberId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });
#pragma warning restore 612, 618
    }
}
