using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixFeedbackEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Shops_ShopId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_ShopId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "StaffName",
                table: "Feedbacks");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountEntityId",
                table: "Feedbacks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "Feedbacks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_AccountEntityId",
                table: "Feedbacks",
                column: "AccountEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_AccountId",
                table: "Feedbacks",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Accounts_AccountEntityId",
                table: "Feedbacks",
                column: "AccountEntityId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Accounts_AccountId",
                table: "Feedbacks",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Accounts_AccountEntityId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Accounts_AccountId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_AccountEntityId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_AccountId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "AccountEntityId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Feedbacks");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Feedbacks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShopId",
                table: "Feedbacks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StaffName",
                table: "Feedbacks",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Anonymous");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_ShopId",
                table: "Feedbacks",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Shops_ShopId",
                table: "Feedbacks",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
