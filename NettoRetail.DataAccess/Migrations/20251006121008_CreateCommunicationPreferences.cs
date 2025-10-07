using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CreateCommunicationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunicationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunicationPreferenceEmail = table.Column<bool>(type: "boolean", nullable: false),
                    CommunicationPreferenceSms = table.Column<bool>(type: "boolean", nullable: false),
                    CommunicationPreferencePush = table.Column<bool>(type: "boolean", nullable: false),
                    OrderUpdateEmail = table.Column<bool>(type: "boolean", nullable: false),
                    OrderUpdateSms = table.Column<bool>(type: "boolean", nullable: false),
                    OrderUpdatePush = table.Column<bool>(type: "boolean", nullable: false),
                    PromotionEmail = table.Column<bool>(type: "boolean", nullable: false),
                    PromotionSms = table.Column<bool>(type: "boolean", nullable: false),
                    PromotionPush = table.Column<bool>(type: "boolean", nullable: false),
                    ShipmentUpdateEmail = table.Column<bool>(type: "boolean", nullable: false),
                    ShipmentUpdateSms = table.Column<bool>(type: "boolean", nullable: false),
                    ShipmentUpdatePush = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunicationPreferences_Accounts_UserId",
                        column: x => x.UserId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationPreferences_UserId",
                table: "CommunicationPreferences",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunicationPreferences");
        }
    }
}
