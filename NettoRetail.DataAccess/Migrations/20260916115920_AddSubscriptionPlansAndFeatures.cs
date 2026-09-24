using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionPlansAndFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubscriptionPlanEntityId",
                table: "Shops",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    MonthlyPrice = table.Column<int>(type: "integer", nullable: false),
                    Moto = table.Column<string>(type: "text", nullable: false),
                    SubscriptionId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeatureEntitySubscriptionPlanEntity",
                columns: table => new
                {
                    FeaturesId = table.Column<int>(type: "integer", nullable: false),
                    SubscriptionPlansId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureEntitySubscriptionPlanEntity", x => new { x.FeaturesId, x.SubscriptionPlansId });
                    table.ForeignKey(
                        name: "FK_FeatureEntitySubscriptionPlanEntity_Features_FeaturesId",
                        column: x => x.FeaturesId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeatureEntitySubscriptionPlanEntity_SubscriptionPlans_Subsc~",
                        column: x => x.SubscriptionPlansId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shops_SubscriptionPlanEntityId",
                table: "Shops",
                column: "SubscriptionPlanEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureEntitySubscriptionPlanEntity_SubscriptionPlansId",
                table: "FeatureEntitySubscriptionPlanEntity",
                column: "SubscriptionPlansId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shops_SubscriptionPlans_SubscriptionPlanEntityId",
                table: "Shops",
                column: "SubscriptionPlanEntityId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shops_SubscriptionPlans_SubscriptionPlanEntityId",
                table: "Shops");

            migrationBuilder.DropTable(
                name: "FeatureEntitySubscriptionPlanEntity");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_Shops_SubscriptionPlanEntityId",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanEntityId",
                table: "Shops");
        }
    }
}
