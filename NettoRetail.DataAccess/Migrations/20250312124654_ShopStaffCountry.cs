using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ShopStaffCountry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Categories_CategoryId",
                table: "InventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Roles_RoleId",
                table: "Staff");

            migrationBuilder.DropTable(
                name: "BlacklistedTokens");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_CategoryId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "LastUpdatedDate",
                table: "InventoryItems");

            migrationBuilder.AddColumn<int>(
                name: "ShopId",
                table: "Staff",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductCategory",
                table: "InventoryItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PhoneCode = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    PhoneMask = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ZipCodeMask = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(90)", maxLength: 90, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Url = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "character varying(90)", maxLength: 90, nullable: false),
                    City = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ZipCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CountryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shops_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Name", "PhoneCode", "PhoneMask", "ZipCodeMask" },
                values: new object[,]
                {
                    { 1, "Austria", "+43", "^\\d{3} \\d{6}$", "^\\d{4}$" },
                    { 2, "Belgium", "+32", "^\\d \\d{3} \\d{2} \\d{2}$", "^\\d{4}$" },
                    { 3, "Bulgaria", "+359", "^\\d{2} \\d{3} \\d{3}$", "^\\d{4}$" },
                    { 4, "Croatia", "+385", "^\\d{2} \\d{3} \\d{4}$", "^\\d{5}$" },
                    { 5, "Cyprus", "+357", "^2\\d \\d{6}$", "^\\d{4}$" },
                    { 6, "Czech Republic", "+420", "^\\d{3} \\d{3} \\d{3}$", "^\\d{5}$" },
                    { 7, "Denmark", "+45", "^\\d{2} \\d{2} \\d{2} \\d{2}$", "^\\d{4}$" },
                    { 8, "Estonia", "+372", "^\\d{4} \\d{4}$", "^\\d{5}$" },
                    { 9, "Finland", "+358", "^\\d \\d{3} \\d{4}$", "^\\d{5}$" },
                    { 10, "France", "+33", "^\\d \\d{2} \\d{2} \\d{2} \\d{2}$", "^\\d{5}$" },
                    { 11, "Germany", "+49", "^\\d{3} \\d{3} \\d{4}$", "^\\d{5}$" },
                    { 12, "Greece", "+30", "^\\d{3} \\d{3} \\d{4}$", "^\\d{3} \\d{2}$" },
                    { 13, "Hungary", "+36", "^\\d{2} \\d{3} \\d{3}$", "^\\d{4}$" },
                    { 14, "Ireland", "+353", "^\\d \\d{3} \\d{4}$", "^[A-Z]\\d{2}$" },
                    { 15, "Italy", "+39", "^\\d{2} \\d{4} \\d{4}$", "^\\d{5}$" },
                    { 16, "Latvia", "+371", "^\\d{2} \\d{3} \\d{3}$", "^LV-\\d{4}$" },
                    { 17, "Lithuania", "+370", "^\\d{3} \\d{5}$", "^LT-\\d{5}$" },
                    { 18, "Luxembourg", "+352", "^\\d{3} \\d{3}$", "^\\d{4}$" },
                    { 19, "Malta", "+356", "^\\d{4} \\d{4}$", "^[A-Z]{3} \\d{4}$" },
                    { 20, "Netherlands", "+31", "^\\d \\d{4} \\d{4}$", "^\\d{4} [A-Z]{2}$" },
                    { 21, "Poland", "+48", "^\\d{3} \\d{3} \\d{3}$", "^\\d{2}-\\d{3}$" },
                    { 22, "Portugal", "+351", "^\\d{3} \\d{3} \\d{3}$", "^\\d{4}-\\d{3}$" },
                    { 23, "Romania", "+40", "^\\d{3} \\d{3} \\d{3}$", "^\\d{6}$" },
                    { 24, "Slovakia", "+421", "^\\d{3} \\d{3} \\d{3}$", "^\\d{3} \\d{2}$" },
                    { 25, "Slovenia", "+386", "^\\d \\d{4} \\d{4}$", "^\\d{4}$" },
                    { 26, "Spain", "+34", "^\\d{3} \\d{3} \\d{3}$", "^\\d{5}$" },
                    { 27, "Sweden", "+46", "^\\d{3} \\d{3} \\d{4}$", "^\\d{3} \\d{2}$" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Staff_ShopId",
                table: "Staff",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Name",
                table: "Countries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_PhoneCode",
                table: "Countries",
                column: "PhoneCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shops_CountryId",
                table: "Shops",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Shops_Email",
                table: "Shops",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shops_Name",
                table: "Shops",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shops_PhoneNumber",
                table: "Shops",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_Roles_RoleId",
                table: "Staff",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_Shops_ShopId",
                table: "Staff",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Roles_RoleId",
                table: "Staff");

            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Shops_ShopId",
                table: "Staff");

            migrationBuilder.DropTable(
                name: "Shops");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Staff_ShopId",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "ProductCategory",
                table: "InventoryItems");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "InventoryItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "InventoryItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedDate",
                table: "InventoryItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BlacklistedTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BlacklistedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlacklistedTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_CategoryId",
                table: "InventoryItems",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Categories_CategoryId",
                table: "InventoryItems",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_Roles_RoleId",
                table: "Staff",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
