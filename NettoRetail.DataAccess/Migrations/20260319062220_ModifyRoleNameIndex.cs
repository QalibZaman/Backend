using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ModifyRoleNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS ux_roles_name_lower;
                """);

            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX ux_roles_name_shop_unique
                ON "Roles" (LOWER("Name"), "ShopId");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS ux_roles_name_shop_unique;
                """);
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX ux_roles_name_lower
                ON "Roles" (LOWER("Name"));
                """);
        }
    }
}
