using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldsFromOrderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalProducts",
                table: "OrderEntity");

            migrationBuilder.DropColumn(
                name: "TotalQuantity",
                table: "OrderEntity");

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "OrderEntity",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerFirstName",
                table: "OrderEntity",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerLastName",
                table: "OrderEntity",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "OrderEntity");

            migrationBuilder.DropColumn(
                name: "CustomerFirstName",
                table: "OrderEntity");

            migrationBuilder.DropColumn(
                name: "CustomerLastName",
                table: "OrderEntity");

            migrationBuilder.AddColumn<int>(
                name: "TotalProducts",
                table: "OrderEntity",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalQuantity",
                table: "OrderEntity",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
