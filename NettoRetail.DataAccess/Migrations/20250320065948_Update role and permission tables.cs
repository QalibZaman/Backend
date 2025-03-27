using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Updateroleandpermissiontables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Shops_ShopId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "IsAllowed",
                table: "Permission_Role");

            migrationBuilder.AlterColumn<int>(
                name: "ShopId",
                table: "Roles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.InsertData(
                table: "PermissionEntity",
                columns: new[] { "Id", "Title" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Inventory management" },
                    { 3, "Customers management" },
                    { 4, "Orders management" },
                    { 5, "Admins management" },
                    { 6, "System settings" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "Name", "ShopId" },
                values: new object[] { 1, "Has full access to the system", "Admin", null });

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Shops_ShopId",
                table: "Roles",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Shops_ShopId",
                table: "Roles");

            migrationBuilder.DeleteData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "ShopId",
                table: "Roles",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAllowed",
                table: "Permission_Role",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Shops_ShopId",
                table: "Roles",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
