using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFieldsToFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Feedbacks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopName",
                table: "Feedbacks",
                type: "character varying(90)",
                maxLength: 90,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffName",
                table: "Feedbacks",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_RoleId",
                table: "Feedbacks",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Roles_RoleId",
                table: "Feedbacks",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Roles_RoleId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_RoleId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "ShopName",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "StaffName",
                table: "Feedbacks");
        }
    }
}
