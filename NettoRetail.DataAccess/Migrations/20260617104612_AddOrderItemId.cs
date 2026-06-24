using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Order_Item",
                table: "Order_Item");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Order_Item",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
        UPDATE "Order_Item"
        SET "Id" = gen_random_uuid()
        WHERE "Id" IS NULL;
    """);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Order_Item",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Order_Item",
                table: "Order_Item",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Item_OrderId_InventoryItemId",
                table: "Order_Item",
                columns: new[] { "OrderId", "InventoryItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Order_Item",
                table: "Order_Item");

            migrationBuilder.DropIndex(
                name: "IX_Order_Item_OrderId_InventoryItemId",
                table: "Order_Item");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Order_Item");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Order_Item",
                table: "Order_Item",
                columns: new[] { "OrderId", "InventoryItemId" });
        }
    }
}
