using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OnBrandDeleteSetNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_BrandEntity_BrandId",
                table: "InventoryItems");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_BrandEntity_BrandId",
                table: "InventoryItems",
                column: "BrandId",
                principalTable: "BrandEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_BrandEntity_BrandId",
                table: "InventoryItems");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_BrandEntity_BrandId",
                table: "InventoryItems",
                column: "BrandId",
                principalTable: "BrandEntity",
                principalColumn: "Id");
        }
    }
}
