using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNullableFieldsToRequiredToFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Roles_RoleId",
                table: "Feedbacks");

            migrationBuilder.AlterColumn<string>(
                name: "StaffName",
                table: "Feedbacks",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShopName",
                table: "Feedbacks",
                type: "character varying(90)",
                maxLength: 90,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(90)",
                oldMaxLength: 90,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                table: "Feedbacks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Roles_RoleId",
                table: "Feedbacks",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Roles_RoleId",
                table: "Feedbacks");

            migrationBuilder.AlterColumn<string>(
                name: "StaffName",
                table: "Feedbacks",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "ShopName",
                table: "Feedbacks",
                type: "character varying(90)",
                maxLength: 90,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(90)",
                oldMaxLength: 90);

            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                table: "Feedbacks",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Roles_RoleId",
                table: "Feedbacks",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");
        }
    }
}
