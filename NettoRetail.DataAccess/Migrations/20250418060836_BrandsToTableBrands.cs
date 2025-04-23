using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class BrandsToTableBrands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BrandEntity_Shops_ShopId",
                table: "BrandEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_BrandEntity_BrandId",
                table: "InventoryItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BrandEntity",
                table: "BrandEntity");

            migrationBuilder.RenameTable(
                name: "BrandEntity",
                newName: "Brands");

            migrationBuilder.RenameIndex(
                name: "IX_BrandEntity_WebsiteUrl",
                table: "Brands",
                newName: "IX_Brands_WebsiteUrl");

            migrationBuilder.RenameIndex(
                name: "IX_BrandEntity_ShopId",
                table: "Brands",
                newName: "IX_Brands_ShopId");

            migrationBuilder.RenameIndex(
                name: "IX_BrandEntity_Name",
                table: "Brands",
                newName: "IX_Brands_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Brands",
                table: "Brands",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Brands_Shops_ShopId",
                table: "Brands",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Brands_BrandId",
                table: "InventoryItems",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Brands_Shops_ShopId",
                table: "Brands");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Brands_BrandId",
                table: "InventoryItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Brands",
                table: "Brands");

            migrationBuilder.RenameTable(
                name: "Brands",
                newName: "BrandEntity");

            migrationBuilder.RenameIndex(
                name: "IX_Brands_WebsiteUrl",
                table: "BrandEntity",
                newName: "IX_BrandEntity_WebsiteUrl");

            migrationBuilder.RenameIndex(
                name: "IX_Brands_ShopId",
                table: "BrandEntity",
                newName: "IX_BrandEntity_ShopId");

            migrationBuilder.RenameIndex(
                name: "IX_Brands_Name",
                table: "BrandEntity",
                newName: "IX_BrandEntity_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BrandEntity",
                table: "BrandEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BrandEntity_Shops_ShopId",
                table: "BrandEntity",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_BrandEntity_BrandId",
                table: "InventoryItems",
                column: "BrandId",
                principalTable: "BrandEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
