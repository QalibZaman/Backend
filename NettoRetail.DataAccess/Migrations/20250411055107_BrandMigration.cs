using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class BrandMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BrandId",
                table: "InventoryItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BrandEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    WebsiteUrl = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LogoImageUrl = table.Column<string>(type: "text", nullable: false),
                    ShopId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BrandEntity_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_BrandId",
                table: "InventoryItems",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_BrandEntity_ShopId",
                table: "BrandEntity",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_BrandEntity_WebsiteUrl",
                table: "BrandEntity",
                column: "WebsiteUrl",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_BrandEntity_BrandId",
                table: "InventoryItems",
                column: "BrandId",
                principalTable: "BrandEntity",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_BrandEntity_BrandId",
                table: "InventoryItems");

            migrationBuilder.DropTable(
                name: "BrandEntity");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_BrandId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "InventoryItems");
        }
    }
}
