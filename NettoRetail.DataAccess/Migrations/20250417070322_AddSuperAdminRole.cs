using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSuperAdminRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "Name", "ShopId" },
                values: new object[] { 99, "System-level admin from RetailTech with full access across tenants", "SuperAdmin", null });

            migrationBuilder.InsertData(
                table: "Permission_Role",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 99 },
                    { 2, 99 },
                    { 3, 99 },
                    { 4, 99 },
                    { 5, 99 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permission_Role",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 99 });

            migrationBuilder.DeleteData(
                table: "Permission_Role",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 99 });

            migrationBuilder.DeleteData(
                table: "Permission_Role",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 99 });

            migrationBuilder.DeleteData(
                table: "Permission_Role",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, 99 });

            migrationBuilder.DeleteData(
                table: "Permission_Role",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 5, 99 });

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 99);
        }
    }
}
