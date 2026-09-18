using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Added_Sub_Start_adn_Renewal_date_nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoRenewalEnabled",
                table: "Shops",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextRenewalDate",
                table: "Shops",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SubscriptionStartDate",
                table: "Shops",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoRenewalEnabled",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "NextRenewalDate",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "SubscriptionStartDate",
                table: "Shops");
        }
    }
}
