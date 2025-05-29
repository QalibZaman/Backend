using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ContentMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentUrl",
                table: "Contents");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Contents",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contents_SectionType_ShopId",
                table: "Contents",
                columns: new[] { "SectionType", "ShopId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contents_SectionType_ShopId",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Contents");

            migrationBuilder.AddColumn<string>(
                name: "ContentUrl",
                table: "Contents",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
