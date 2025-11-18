using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationshipsBetweenShopAndSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_SubscriptionPlans_SubscriptionPlanId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_SubscriptionPlanId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Subscription",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanId",
                table: "Accounts");

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionPlanId",
                table: "Shops",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Shops_SubscriptionPlanId",
                table: "Shops",
                column: "SubscriptionPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shops_SubscriptionPlans_SubscriptionPlanId",
                table: "Shops",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shops_SubscriptionPlans_SubscriptionPlanId",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Shops_SubscriptionPlanId",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanId",
                table: "Shops");

            migrationBuilder.AddColumn<string>(
                name: "Subscription",
                table: "Shops",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionPlanId",
                table: "Accounts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_SubscriptionPlanId",
                table: "Accounts",
                column: "SubscriptionPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_SubscriptionPlans_SubscriptionPlanId",
                table: "Accounts",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id");
        }
    }
}
