using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCategresSectionFieldsToShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoriesSectionName",
                table: "Shops",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoriesSectionPreview",
                table: "Shops",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CategoriesSectionVisibility",
                table: "Shops",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoriesSectionName",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "CategoriesSectionPreview",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "CategoriesSectionVisibility",
                table: "Shops");
        }
    }
}
