using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Cofigureroleandpermissiontables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.UpdateData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 1,
                column: "Title",
                value: "Inventory management");

            migrationBuilder.UpdateData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 2,
                column: "Title",
                value: "Customers management");

            migrationBuilder.UpdateData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 3,
                column: "Title",
                value: "Orders management");

            migrationBuilder.UpdateData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 4,
                column: "Title",
                value: "Admins management");

            migrationBuilder.UpdateData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 5,
                column: "Title",
                value: "System settings");

            migrationBuilder.InsertData(
                table: "Permission_Role",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permission_Role",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "Permission_Role",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "Permission_Role",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "Permission_Role",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, 1 });

            migrationBuilder.DeleteData(
                table: "Permission_Role",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5, 1 });

            migrationBuilder.UpdateData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 1,
                column: "Title",
                value: "Admin");

            migrationBuilder.UpdateData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 2,
                column: "Title",
                value: "Inventory management");

            migrationBuilder.UpdateData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 3,
                column: "Title",
                value: "Customers management");

            migrationBuilder.UpdateData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 4,
                column: "Title",
                value: "Orders management");

            migrationBuilder.UpdateData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 5,
                column: "Title",
                value: "Admins management");

            migrationBuilder.InsertData(
                table: "PermissionEntity",
                columns: new[] { "Id", "Title" },
                values: new object[] { 6, "System settings" });
        }
    }
}
