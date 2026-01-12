using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryItemSelfRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasVariations",
                table: "InventoryItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVariation",
                table: "InventoryItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentItemId",
                table: "InventoryItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ParentItemId",
                table: "InventoryItems",
                column: "ParentItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_InventoryItems_ParentItemId",
                table: "InventoryItems",
                column: "ParentItemId",
                principalTable: "InventoryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_InventoryItems_ParentItemId",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_ParentItemId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "HasVariations",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "IsVariation",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "ParentItemId",
                table: "InventoryItems");
        }
    }
}
