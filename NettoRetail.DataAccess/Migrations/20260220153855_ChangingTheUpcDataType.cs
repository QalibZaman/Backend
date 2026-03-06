using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangingTheUpcDataType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProductUpc",
                table: "InventoryItems",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ProductUpc",
                table: "CartItems",
                type: "text",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProductUpc",
                table: "InventoryItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(12)",
                oldMaxLength: 12);

            migrationBuilder.AlterColumn<long>(
                name: "ProductUpc",
                table: "CartItems",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
