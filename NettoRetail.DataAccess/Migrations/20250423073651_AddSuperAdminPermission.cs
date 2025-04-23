using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSuperAdminPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PermissionEntity",
                columns: new[] { "Id", "Title" },
                values: new object[] { 6, "SuperAdmins management" });

            migrationBuilder.InsertData(
                table: "Permission_Role",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 6, 99 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permission_Role",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 6, 99 });

            migrationBuilder.DeleteData(
                table: "PermissionEntity",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
