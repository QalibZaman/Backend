using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixAllMigrationIssues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlacklistedTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    BlacklistedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlacklistedTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    URL = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Visibility = table.Column<bool>(type: "boolean", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductUpc = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AvailableAmount = table.Column<int>(type: "integer", nullable: false),
                    ReservedAmount = table.Column<int>(type: "integer", nullable: false),
                    OnHandAmount = table.Column<int>(type: "integer", nullable: false),
                    ProductLocation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LastName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ShopId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Staff_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Staff_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FailedAuthAttempt",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    FailedAttempts = table.Column<int>(type: "integer", nullable: false),
                    LockoutEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastFailedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailedAuthAttempt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FailedAuthAttempt_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Jwt = table.Column<string>(type: "text", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: false),
                    Device = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Browser = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastActivityTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Staff_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "RoleName" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "SalesManager" },
                    { 3, "Customer" },
                    { 4, "InventoryManager" },
                    { 5, "PosCashier" },
                    { 6, "ManagementStakeholder" },
                    { 7, "Shipper" },
                    { 8, "CustomerSupportSpecialist" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId",
                table: "Categories",
                column: "ParentId");

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
                name: "IX_FailedAuthAttempt_StaffId",
                table: "FailedAuthAttempt",
                column: "StaffId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_CategoryId",
                table: "InventoryItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ProductUpc",
                table: "InventoryItems",
                column: "ProductUpc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_AccountId",
                table: "Sessions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_Jwt",
                table: "Sessions",
                column: "Jwt",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_RefreshToken",
                table: "Sessions",
                column: "RefreshToken",
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

            migrationBuilder.CreateIndex(
                name: "IX_Staff_RoleId",
                table: "Staff",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_ShopId",
                table: "Staff",
                column: "ShopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlacklistedTokens");

            migrationBuilder.DropTable(
                name: "FailedAuthAttempt");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Shops");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
