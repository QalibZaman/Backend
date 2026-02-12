using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ImplementedLocksTableAndRemovedOutdatedLocksProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Accounts_LockedByUserId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_LockedByUserId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "LockedAtUtc",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "LockedByUserId",
                table: "Roles");

            migrationBuilder.CreateTable(
                name: "EntityLocks",
                columns: table => new
                {
                    ResourceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ResourceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShopId = table.Column<int>(type: "integer", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LockedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityLocks", x => new { x.ResourceType, x.ResourceId, x.ShopId });
                    table.ForeignKey(
                        name: "FK_EntityLocks_Accounts_LockedByUserId",
                        column: x => x.LockedByUserId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntityLocks_LockedByUserId",
                table: "EntityLocks",
                column: "LockedByUserId");

            migrationBuilder.CreateIndex(
                name: "UX_EntityLocks_Resource",
                table: "EntityLocks",
                columns: new[] { "ResourceType", "ResourceId", "ShopId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntityLocks");

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedAtUtc",
                table: "Roles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LockedByUserId",
                table: "Roles",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "LockedAtUtc", "LockedByUserId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "LockedAtUtc", "LockedByUserId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 99,
                columns: new[] { "LockedAtUtc", "LockedByUserId" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_LockedByUserId",
                table: "Roles",
                column: "LockedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Accounts_LockedByUserId",
                table: "Roles",
                column: "LockedByUserId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
