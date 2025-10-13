using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddDeleteJobIdToCartItemTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeletionJobId",
                table: "CartItems",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletionJobId",
                table: "CartItems");
        }
    }
}
