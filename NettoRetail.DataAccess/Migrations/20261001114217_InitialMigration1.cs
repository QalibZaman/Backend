using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionPlanId",
                table: "Shops");

            migrationBuilder.AddColumn<bool>(
                name: "AutoRenewalEnabled",
                table: "Subscriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoRenewalEnabled",
                table: "Subscriptions");

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionPlanId",
                table: "Shops",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
