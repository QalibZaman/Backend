using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangePlatformAdminTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlatformAdminEntity_Accounts_AccountId",
                table: "PlatformAdminEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlatformAdminEntity",
                table: "PlatformAdminEntity");

            migrationBuilder.RenameTable(
                name: "PlatformAdminEntity",
                newName: "PlatformAdmins");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlatformAdmins",
                table: "PlatformAdmins",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformAdmins_Accounts_AccountId",
                table: "PlatformAdmins",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlatformAdmins_Accounts_AccountId",
                table: "PlatformAdmins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlatformAdmins",
                table: "PlatformAdmins");

            migrationBuilder.RenameTable(
                name: "PlatformAdmins",
                newName: "PlatformAdminEntity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlatformAdminEntity",
                table: "PlatformAdminEntity",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformAdminEntity_Accounts_AccountId",
                table: "PlatformAdminEntity",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
