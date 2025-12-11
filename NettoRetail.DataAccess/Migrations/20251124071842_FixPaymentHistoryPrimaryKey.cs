using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixPaymentHistoryPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentHistories",
                table: "PaymentHistories");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "PaymentHistories",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentHistories",
                table: "PaymentHistories",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistories_AccountId",
                table: "PaymentHistories",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentHistories",
                table: "PaymentHistories");

            migrationBuilder.DropIndex(
                name: "IX_PaymentHistories_AccountId",
                table: "PaymentHistories");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PaymentHistories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentHistories",
                table: "PaymentHistories",
                column: "AccountId");
        }
    }
}
