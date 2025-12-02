using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixPermissionRoleSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            DELETE FROM ""Permission_Role""
            WHERE (""RoleId"" = 1 AND ""PermissionId"" IN (1,2,3,4,5))
            OR (""RoleId"" = 99 AND ""PermissionId"" IN (1,2,3,4,5,6));
            ");

            migrationBuilder.Sql(@"
            INSERT INTO ""Permission_Role"" (""RoleId"", ""PermissionId"")
            VALUES
            (1,1),(1,2),(1,3),(1,4),(1,5),
            (99,1),(99,2),(99,3),(99,4),(99,5),(99,6);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            DELETE FROM ""Permission_Role""
            WHERE (""RoleId"" = 1 AND ""PermissionId"" IN (1,2,3,4,5))
            OR (""RoleId"" = 99 AND ""PermissionId"" IN (1,2,3,4,5,6));
            ");
        }
    }
}
