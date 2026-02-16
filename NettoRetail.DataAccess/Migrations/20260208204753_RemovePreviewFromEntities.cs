using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemovePreviewFromEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoriesSectionPreview",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "Preview",
                table: "OfferSections");

            migrationBuilder.DropColumn(
                name: "Preview",
                table: "NewProductsSections");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoriesSectionPreview",
                table: "Shops",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Preview",
                table: "OfferSections",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Preview",
                table: "NewProductsSections",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}
