using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddComparisonListAndItemsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComparisonLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparisonLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComparisonLists_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComparisonListItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComparisonListId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparisonListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComparisonListItems_ComparisonLists_ComparisonListId",
                        column: x => x.ComparisonListId,
                        principalTable: "ComparisonLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComparisonListItems_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonListItems_ComparisonListId",
                table: "ComparisonListItems",
                column: "ComparisonListId");

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonListItems_InventoryItemId",
                table: "ComparisonListItems",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonLists_CustomerId",
                table: "ComparisonLists",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComparisonListItems");

            migrationBuilder.DropTable(
                name: "ComparisonLists");
        }
    }
}
