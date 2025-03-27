using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Adding_Permission_entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PermissionEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permission_Role",
                columns: table => new
                {
                    PermissionEntitiesId = table.Column<int>(type: "integer", nullable: false),
                    RoleEntitiesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission_Role", x => new { x.PermissionEntitiesId, x.RoleEntitiesId });
                    table.ForeignKey(
                        name: "FK_Permission_Role_PermissionEntity_PermissionEntitiesId",
                        column: x => x.PermissionEntitiesId,
                        principalTable: "PermissionEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Permission_Role_Roles_RoleEntitiesId",
                        column: x => x.RoleEntitiesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Role_RoleEntitiesId",
                table: "Permission_Role",
                column: "RoleEntitiesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Permission_Role");

            migrationBuilder.DropTable(
                name: "PermissionEntity");
        }
    }
}
