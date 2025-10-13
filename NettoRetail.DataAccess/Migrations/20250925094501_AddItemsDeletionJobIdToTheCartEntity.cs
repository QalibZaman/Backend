using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddItemsDeletionJobIdToTheCartEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemsDeletionJobId",
                table: "Carts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemsDeletionJobId",
                table: "Carts");
        }
    }
}
