using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddComparisonTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comparison_lists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comparison_lists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "comparison_list_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    PromotionPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Brand = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsPromoted = table.Column<bool>(type: "boolean", nullable: false),
                    AvailableAmount = table.Column<int>(type: "integer", nullable: false),
                    ReservedAmount = table.Column<int>(type: "integer", nullable: false),
                    OnHandAmount = table.Column<int>(type: "integer", nullable: false),
                    ProductLocation = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    InventoryItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComparisonListId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comparison_list_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comparison_list_items_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_comparison_list_items_comparison_lists_ComparisonListId",
                        column: x => x.ComparisonListId,
                        principalTable: "comparison_lists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_comparison_list_items_ComparisonListId_InventoryItemId",
                table: "comparison_list_items",
                columns: new[] { "ComparisonListId", "InventoryItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_comparison_list_items_InventoryItemId",
                table: "comparison_list_items",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_comparison_lists_UserId_CategoryId",
                table: "comparison_lists",
                columns: new[] { "UserId", "CategoryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comparison_list_items");

            migrationBuilder.DropTable(
                name: "comparison_lists");
        }
    }
}
