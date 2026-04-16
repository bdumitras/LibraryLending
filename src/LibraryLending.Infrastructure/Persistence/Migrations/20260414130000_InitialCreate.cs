using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryLending.Infrastructure.Persistence.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "books",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Author = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                Isbn = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                PublicationYear = table.Column<int>(type: "integer", nullable: false),
                TotalCopies = table.Column<int>(type: "integer", nullable: false),
                AvailableCopies = table.Column<int>(type: "integer", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_books", x => x.Id);
                table.CheckConstraint("CK_Books_TotalCopies_Positive", "\"TotalCopies\" > 0");
                table.CheckConstraint("CK_Books_AvailableCopies_NonNegative", "\"AvailableCopies\" >= 0");
                table.CheckConstraint("CK_Books_AvailableCopies_NotGreaterThan_TotalCopies", "\"AvailableCopies\" <= \"TotalCopies\"");
            });

        migrationBuilder.CreateTable(
            name: "members",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_members", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "loans",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                BookId = table.Column<Guid>(type: "uuid", nullable: false),
                MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                LoanDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                DueDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ReturnedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_loans", x => x.Id);
                table.CheckConstraint("CK_Loans_DueDate_After_LoanDate", "\"DueDateUtc\" > \"LoanDateUtc\"");
                table.CheckConstraint("CK_Loans_ReturnedAt_After_Or_Equal_LoanDate", "\"ReturnedAtUtc\" IS NULL OR \"ReturnedAtUtc\" >= \"LoanDateUtc\"");
                table.CheckConstraint("CK_Loans_Status_Valid", "\"Status\" = 'Active' OR \"Status\" = 'Returned' OR \"Status\" = 'Overdue'");
                table.ForeignKey(
                    name: "FK_loans_books_BookId",
                    column: x => x.BookId,
                    principalTable: "books",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_loans_members_MemberId",
                    column: x => x.MemberId,
                    principalTable: "members",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_books_Author",
            table: "books",
            column: "Author");

        migrationBuilder.CreateIndex(
            name: "IX_books_Isbn",
            table: "books",
            column: "Isbn",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_books_Title",
            table: "books",
            column: "Title");

        migrationBuilder.CreateIndex(
            name: "IX_loans_BookId",
            table: "loans",
            column: "BookId");

        migrationBuilder.CreateIndex(
            name: "IX_loans_BookId_Status",
            table: "loans",
            columns: new[] { "BookId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_loans_DueDateUtc",
            table: "loans",
            column: "DueDateUtc");

        migrationBuilder.CreateIndex(
            name: "IX_loans_MemberId",
            table: "loans",
            column: "MemberId");

        migrationBuilder.CreateIndex(
            name: "IX_loans_MemberId_Status",
            table: "loans",
            columns: new[] { "MemberId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_loans_Status",
            table: "loans",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_members_Email",
            table: "members",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_members_FullName",
            table: "members",
            column: "FullName");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "loans");

        migrationBuilder.DropTable(
            name: "books");

        migrationBuilder.DropTable(
            name: "members");
    }
}
