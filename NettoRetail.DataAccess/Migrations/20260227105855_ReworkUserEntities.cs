using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ReworkUserEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Customers_CustomerId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunicationPreferences_Accounts_UserId",
                table: "CommunicationPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Accounts_AccountId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityLocks_Accounts_LockedByUserId",
                table: "EntityLocks");

            migrationBuilder.DropForeignKey(
                name: "FK_FailedAuthAttempt_Accounts_AccountId",
                table: "FailedAuthAttempt");

            migrationBuilder.DropForeignKey(
                name: "FK_FavouriteListItems_Accounts_AccountId",
                table: "FavouriteListItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Accounts_AccountEntityId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Accounts_AccountId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_InvitationTokens_Accounts_AccountId",
                table: "InvitationTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCommunicationPreferences_Accounts_UserId",
                table: "UserCommunicationPreferences");

            migrationBuilder.DropTable(
                name: "UserCommunicationPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemComparison_Accounts_UserId",
                table: "ItemComparison");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatusHistories_Accounts_UpdatedById",
                table: "OrderStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PasswordHistories_Accounts_AccountId",
                table: "PasswordHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentHistories_Accounts_AccountId",
                table: "PaymentHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethods_Accounts_AccountId",
                table: "PaymentMethods");

            migrationBuilder.DropForeignKey(
                name: "FK_RecentlyViewedProducts_Accounts_AccountId",
                table: "RecentlyViewedProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Accounts_AccountId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Accounts_AccountId",
                table: "Sessions");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_AccountEntityId",
                table: "Feedbacks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Customers",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AccountEntityId",
                table: "Feedbacks");

            migrationBuilder.RenameTable(
                name: "Customers",
                newName: "Users");

            migrationBuilder.Sql(@"
                DELETE FROM ""CommunicationPreferences""
                WHERE ""UserId"" NOT IN (SELECT ""Id"" FROM ""Users"");");

            migrationBuilder.Sql(@"
                DELETE FROM ""OrderStatusHistories""
                WHERE ""UpdatedById"" NOT IN (SELECT ""Id"" FROM ""Users"");");

            migrationBuilder.Sql(@"
                DELETE FROM ""PaymentMethods"";");

            migrationBuilder.Sql(@"
                DELETE FROM ""Sessions"";");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "Reviews",
                newName: "StaffId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_AccountId",
                table: "Reviews",
                newName: "IX_Reviews_StaffId");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "Users",
                newName: "Id");

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedById",
                table: "OrderStatusHistories",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Users",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EncryptedTotpSecret",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsOnboardingCompleted",
                table: "Users",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubscribedToNewsletter",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordUpdatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                table: "Users",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShopId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerId",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "Users",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql(@"
                DELETE FROM ""Users""
                WHERE ""RoleId"" = 0;");

            migrationBuilder.Sql(@"
                DELETE FROM ""FailedAuthAttempt"";");

            migrationBuilder.Sql(@"
                DELETE FROM ""Orders"";");

            migrationBuilder.Sql(@"
                DELETE FROM ""Reviews"";");

            migrationBuilder.Sql(@"
                DELETE FROM ""Feedbacks"";");

            migrationBuilder.Sql(@"
                DELETE FROM ""Carts"";");

            migrationBuilder.Sql(@"
                DELETE FROM ""Contents"";");

            migrationBuilder.Sql(@"
                DELETE FROM ""EntityLocks"";");

            migrationBuilder.Sql(@"
                DELETE FROM ""FavouriteListItems"";");

            migrationBuilder.Sql(@"
                DELETE FROM ""InvitationTokens"";");

            migrationBuilder.Sql(@"
                DELETE FROM ""ItemComparison_Item"";");

            migrationBuilder.Sql(@"
                DELETE FROM ""ItemComparison"";");

            migrationBuilder.Sql(@"
                DELETE FROM ""PasswordHistories"";");

            migrationBuilder.Sql(@"
                DELETE FROM ""PaymentHistories"";");

            migrationBuilder.Sql(@"
                DELETE FROM ""PriceHistories"";");

            migrationBuilder.Sql(@"
                DELETE FROM ""RecentlyViewedProducts"";");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CountryId",
                table: "Users",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email_StaffOnly",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "\"Discriminator\" = 'StaffEntity'");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ShopId",
                table: "Users",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Users_CustomerId",
                table: "Carts",
                column: "CustomerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommunicationPreferences_Users_UserId",
                table: "CommunicationPreferences",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityLocks_Users_LockedByUserId",
                table: "EntityLocks",
                column: "LockedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FailedAuthAttempt_Users_AccountId",
                table: "FailedAuthAttempt",
                column: "AccountId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FavouriteListItems_Users_AccountId",
                table: "FavouriteListItems",
                column: "AccountId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Users_AccountId",
                table: "Feedbacks",
                column: "AccountId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationTokens_Users_AccountId",
                table: "InvitationTokens",
                column: "AccountId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemComparison_Users_UserId",
                table: "ItemComparison",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStatusHistories_Users_UpdatedById",
                table: "OrderStatusHistories",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordHistories_Users_AccountId",
                table: "PasswordHistories",
                column: "AccountId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentHistories_Users_AccountId",
                table: "PaymentHistories",
                column: "AccountId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethods_Users_AccountId",
                table: "PaymentMethods",
                column: "AccountId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecentlyViewedProducts_Users_AccountId",
                table: "RecentlyViewedProducts",
                column: "AccountId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_StaffId",
                table: "Reviews",
                column: "StaffId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Users_AccountId",
                table: "Sessions",
                column: "AccountId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Countries_CountryId",
                table: "Users",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Shops_ShopId",
                table: "Users",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Users_CustomerId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunicationPreferences_Users_UserId",
                table: "CommunicationPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityLocks_Users_LockedByUserId",
                table: "EntityLocks");

            migrationBuilder.DropForeignKey(
                name: "FK_FailedAuthAttempt_Users_AccountId",
                table: "FailedAuthAttempt");

            migrationBuilder.DropForeignKey(
                name: "FK_FavouriteListItems_Users_AccountId",
                table: "FavouriteListItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Users_AccountId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_InvitationTokens_Users_AccountId",
                table: "InvitationTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemComparison_Users_UserId",
                table: "ItemComparison");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_CustomerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatusHistories_Users_UpdatedById",
                table: "OrderStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PasswordHistories_Users_AccountId",
                table: "PasswordHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentHistories_Users_AccountId",
                table: "PaymentHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethods_Users_AccountId",
                table: "PaymentMethods");

            migrationBuilder.DropForeignKey(
                name: "FK_RecentlyViewedProducts_Users_AccountId",
                table: "RecentlyViewedProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_StaffId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Users_AccountId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Countries_CountryId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Shops_ShopId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CountryId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email_StaffOnly",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ShopId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EncryptedTotpSecret",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsOnboardingCompleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsSubscribedToNewsletter",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordUpdatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StripeCustomerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Customers");

            migrationBuilder.RenameColumn(
                name: "StaffId",
                table: "Reviews",
                newName: "AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_StaffId",
                table: "Reviews",
                newName: "IX_Reviews_AccountId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Customers",
                newName: "AccountId");

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedById",
                table: "OrderStatusHistories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AccountEntityId",
                table: "Feedbacks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Customers",
                table: "Customers",
                column: "AccountId");

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryId = table.Column<int>(type: "integer", nullable: true),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ShopId = table.Column<int>(type: "integer", nullable: true),
                    City = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EncryptedTotpSecret = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsOnboardingCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsSubscribedToNewsletter = table.Column<bool>(type: "boolean", nullable: false),
                    LastName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "text", nullable: true),
                    ShippingAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    StripeCustomerId = table.Column<string>(type: "text", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ZipCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_AccountEntityId",
                table: "Feedbacks",
                column: "AccountEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CountryId",
                table: "Accounts",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Email",
                table: "Accounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_RoleId",
                table: "Accounts",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ShopId",
                table: "Accounts",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Customers_CustomerId",
                table: "Carts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommunicationPreferences_Accounts_UserId",
                table: "CommunicationPreferences",
                column: "UserId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Accounts_AccountId",
                table: "Customers",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityLocks_Accounts_LockedByUserId",
                table: "EntityLocks",
                column: "LockedByUserId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FailedAuthAttempt_Accounts_AccountId",
                table: "FailedAuthAttempt",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FavouriteListItems_Accounts_AccountId",
                table: "FavouriteListItems",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Accounts_AccountEntityId",
                table: "Feedbacks",
                column: "AccountEntityId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Accounts_AccountId",
                table: "Feedbacks",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationTokens_Accounts_AccountId",
                table: "InvitationTokens",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemComparison_Accounts_UserId",
                table: "ItemComparison",
                column: "UserId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStatusHistories_Accounts_UpdatedById",
                table: "OrderStatusHistories",
                column: "UpdatedById",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordHistories_Accounts_AccountId",
                table: "PasswordHistories",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentHistories_Accounts_AccountId",
                table: "PaymentHistories",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethods_Accounts_AccountId",
                table: "PaymentMethods",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecentlyViewedProducts_Accounts_AccountId",
                table: "RecentlyViewedProducts",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Accounts_AccountId",
                table: "Reviews",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Accounts_AccountId",
                table: "Sessions",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
