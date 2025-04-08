using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenameOrdersEntityTableToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Item_OrderEntity_OrderId",
                table: "Order_Item");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderEntity_Customers_CustomerId",
                table: "OrderEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderEntity",
                table: "OrderEntity");

            migrationBuilder.RenameTable(
                name: "OrderEntity",
                newName: "Orders");

            migrationBuilder.RenameIndex(
                name: "IX_OrderEntity_CustomerId",
                table: "Orders",
                newName: "IX_Orders_CustomerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orders",
                table: "Orders",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Item_Orders_OrderId",
                table: "Order_Item",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Item_Orders_OrderId",
                table: "Order_Item");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orders",
                table: "Orders");

            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "OrderEntity");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_CustomerId",
                table: "OrderEntity",
                newName: "IX_OrderEntity_CustomerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderEntity",
                table: "OrderEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Item_OrderEntity_OrderId",
                table: "Order_Item",
                column: "OrderId",
                principalTable: "OrderEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderEntity_Customers_CustomerId",
                table: "OrderEntity",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "AccountId");
        }
    }
}
