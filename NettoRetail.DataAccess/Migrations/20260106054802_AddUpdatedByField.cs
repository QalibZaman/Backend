using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedByField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedById",
                table: "OrderStatusHistories",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "OrderStatusHistories"
                SET "UpdatedById" = (
                    SELECT "Id" FROM "Accounts" ORDER BY "Id" LIMIT 1
                )
                WHERE "UpdatedById" IS NULL;
            """);

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedById",
                table: "OrderStatusHistories",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_UpdatedById",
                table: "OrderStatusHistories",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStatusHistories_Accounts_UpdatedById",
                table: "OrderStatusHistories",
                column: "UpdatedById",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatusHistories_Accounts_UpdatedById",
                table: "OrderStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_OrderStatusHistories_UpdatedById",
                table: "OrderStatusHistories");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "OrderStatusHistories");
        }
    }
}
