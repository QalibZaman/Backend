using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Initialmigration : Migration
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
                    BlacklistedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlacklistedTokens", x => x.Id);
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
                name: "PermissionEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionEntity", x => x.Id);
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
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LogoImageUrl = table.Column<string>(type: "text", nullable: true),
                    ShopId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Brands_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    ShopId = table.Column<int>(type: "integer", nullable: false)
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
                    table.ForeignKey(
                        name: "FK_Categories_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ShopId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id");
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
                    PromotionPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IsPromoted = table.Column<bool>(type: "boolean", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AvailableAmount = table.Column<int>(type: "integer", nullable: false),
                    ReservedAmount = table.Column<int>(type: "integer", nullable: false),
                    OnHandAmount = table.Column<int>(type: "integer", nullable: false),
                    ProductLocation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShopId = table.Column<int>(type: "integer", nullable: false),
                    BrandId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryItems_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryItems_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LastName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    ShippingAddress = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    ZipCode = table.Column<string>(type: "text", nullable: true),
                    CountryId = table.Column<int>(type: "integer", nullable: true),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ProfileImageUrl = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    EncryptedTotpSecret = table.Column<string>(type: "text", nullable: true),
                    PasswordUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ShopId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accounts_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accounts_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Permission_Role",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission_Role", x => new { x.PermissionId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_Permission_Role_PermissionEntity_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "PermissionEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Permission_Role_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LoyaltyId = table.Column<string>(type: "text", nullable: true),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_Customers_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FailedAuthAttempt",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    FailedAttempts = table.Column<int>(type: "integer", nullable: false),
                    LockoutEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastFailedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailedAuthAttempt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FailedAuthAttempt_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
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
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Creator = table.Column<string>(type: "text", nullable: false),
                    OrderStatus = table.Column<string>(type: "text", nullable: false),
                    DeliveryType = table.Column<string>(type: "text", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Order_Item",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_Item", x => new { x.OrderId, x.ItemId });
                    table.ForeignKey(
                        name: "FK_Order_Item_InventoryItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_Item_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    OldStatus = table.Column<int>(type: "integer", nullable: false),
                    NewStatus = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistories_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
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
                table: "PermissionEntity",
                columns: new[] { "Id", "Title" },
                values: new object[,]
                {
                    { 1, "Inventory management" },
                    { 2, "Customers management" },
                    { 3, "Orders management" },
                    { 4, "Admins management" },
                    { 5, "System settings" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "Name", "ShopId" },
                values: new object[,]
                {
                    { 1, "Has full access to the system", "Admin", null },
                    { 2, "Has access to only what the customer can do", "Customer", null },
                    { 99, "System-level admin from RetailTech with full access across tenants", "SuperAdmin", null }
                });

            migrationBuilder.InsertData(
                table: "Permission_Role",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 99 },
                    { 2, 1 },
                    { 2, 99 },
                    { 3, 1 },
                    { 3, 99 },
                    { 4, 1 },
                    { 4, 99 },
                    { 5, 1 },
                    { 5, 99 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CountryId",
                table: "Accounts",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_RoleId",
                table: "Accounts",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ShopId",
                table: "Accounts",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Name",
                table: "Brands",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Brands_ShopId",
                table: "Brands",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_WebsiteUrl",
                table: "Brands",
                column: "WebsiteUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId",
                table: "Categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ShopId",
                table: "Categories",
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
                name: "IX_FailedAuthAttempt_AccountId",
                table: "FailedAuthAttempt",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_BrandId",
                table: "InventoryItems",
                column: "BrandId");

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
                name: "IX_InventoryItems_ShopId",
                table: "InventoryItems",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Item_ItemId",
                table: "Order_Item",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_OrderId",
                table: "OrderStatusHistories",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Role_RoleId",
                table: "Permission_Role",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_ShopId",
                table: "Roles",
                column: "ShopId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlacklistedTokens");

            migrationBuilder.DropTable(
                name: "FailedAuthAttempt");

            migrationBuilder.DropTable(
                name: "Order_Item");

            migrationBuilder.DropTable(
                name: "OrderStatusHistories");

            migrationBuilder.DropTable(
                name: "Permission_Role");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "PermissionEntity");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Shops");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
