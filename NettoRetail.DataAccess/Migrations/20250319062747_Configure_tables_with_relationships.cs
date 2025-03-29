using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Configure_tables_with_relationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permission_Role_PermissionEntity_PermissionEntitiesId",
                table: "Permission_Role");

            migrationBuilder.DropForeignKey(
                name: "FK_Permission_Role_Roles_RoleEntitiesId",
                table: "Permission_Role");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DropColumn(
                name: "RoleName",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PermissionEntity");

            migrationBuilder.RenameColumn(
                name: "RoleEntitiesId",
                table: "Permission_Role",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "PermissionEntitiesId",
                table: "Permission_Role",
                newName: "PermissionId");

            migrationBuilder.RenameIndex(
                name: "IX_Permission_Role_RoleEntitiesId",
                table: "Permission_Role",
                newName: "IX_Permission_Role_RoleId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Roles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Roles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ShopId",
                table: "Roles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "PermissionEntity",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "IsAllowed",
                table: "Permission_Role",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ShopId",
                table: "Categories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_ShopId",
                table: "Roles",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ShopId",
                table: "Categories",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Shops_ShopId",
                table: "Categories",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Permission_Role_PermissionEntity_PermissionId",
                table: "Permission_Role",
                column: "PermissionId",
                principalTable: "PermissionEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Permission_Role_Roles_RoleId",
                table: "Permission_Role",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Shops_ShopId",
                table: "Roles",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Shops_ShopId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Permission_Role_PermissionEntity_PermissionId",
                table: "Permission_Role");

            migrationBuilder.DropForeignKey(
                name: "FK_Permission_Role_Roles_RoleId",
                table: "Permission_Role");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Shops_ShopId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_ShopId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ShopId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "IsAllowed",
                table: "Permission_Role");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "Permission_Role",
                newName: "RoleEntitiesId");

            migrationBuilder.RenameColumn(
                name: "PermissionId",
                table: "Permission_Role",
                newName: "PermissionEntitiesId");

            migrationBuilder.RenameIndex(
                name: "IX_Permission_Role_RoleId",
                table: "Permission_Role",
                newName: "IX_Permission_Role_RoleEntitiesId");

            migrationBuilder.AddColumn<string>(
                name: "RoleName",
                table: "Roles",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "PermissionEntity",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PermissionEntity",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Permission_Role_PermissionEntity_PermissionEntitiesId",
                table: "Permission_Role",
                column: "PermissionEntitiesId",
                principalTable: "PermissionEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Permission_Role_Roles_RoleEntitiesId",
                table: "Permission_Role",
                column: "RoleEntitiesId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
