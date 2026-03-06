using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixRoleLockedByRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Roles_LockedByUserId",
                table: "Roles",
                column: "LockedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Accounts_LockedByUserId",
                table: "Roles",
                column: "LockedByUserId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Accounts_LockedByUserId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_LockedByUserId",
                table: "Roles");
        }
    }
}
