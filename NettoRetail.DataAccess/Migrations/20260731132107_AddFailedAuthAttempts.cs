using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddFailedAuthAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FailedAuthAttemptEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    FailedAttempts = table.Column<int>(type: "integer", nullable: false),
                    LockoutEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastFailedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailedAuthAttemptEntity", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FailedAuthAttemptEntity_AccountId",
                table: "FailedAuthAttemptEntity",
                column: "AccountId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FailedAuthAttemptEntity");
        }
    }
}
