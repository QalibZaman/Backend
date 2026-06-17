using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    public partial class MigrateOrderItemInventoryItemId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Item_InventoryItems_ItemId",
                table: "Order_Item");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Order_Item",
                table: "Order_Item");

            migrationBuilder.DropIndex(
                name: "IX_Order_Item_ItemId",
                table: "Order_Item");

            migrationBuilder.Sql("""
                UPDATE "Order_Item"
                SET "InventoryItemId" = "ItemId"
                WHERE "InventoryItemId" = '00000000-0000-0000-0000-000000000000';
            """);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Order_Item",
                table: "Order_Item",
                columns: new[] { "OrderId", "InventoryItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_Order_Item_InventoryItemId",
                table: "Order_Item",
                column: "InventoryItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Item_InventoryItems_InventoryItemId",
                table: "Order_Item",
                column: "InventoryItemId",
                principalTable: "InventoryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Item_InventoryItems_InventoryItemId",
                table: "Order_Item");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Order_Item",
                table: "Order_Item");

            migrationBuilder.DropIndex(
                name: "IX_Order_Item_InventoryItemId",
                table: "Order_Item");

            migrationBuilder.Sql("""
                UPDATE "Order_Item"
                SET "ItemId" = "InventoryItemId";
            """);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Order_Item",
                table: "Order_Item",
                columns: new[] { "OrderId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_Order_Item_ItemId",
                table: "Order_Item",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Item_InventoryItems_ItemId",
                table: "Order_Item",
                column: "ItemId",
                principalTable: "InventoryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}