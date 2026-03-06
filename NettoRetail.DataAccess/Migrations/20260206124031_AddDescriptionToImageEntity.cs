using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToImageEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "InventoryItemImages",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Test 
            migrationBuilder.Sql("""
                UPDATE "InventoryItemImages" iii
                SET "Description" = 'Image related to ' || ii."ProductName"
                FROM "InventoryItems" ii
                WHERE iii."InventoryItemId" = ii."Id"
                    AND iii."Description" = '';
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "InventoryItemImages");
        }
    }
}
