using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTableNameAddressEntitiesToAddresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddressEntities_Countries_CountryId",
                table: "AddressEntities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AddressEntities",
                table: "AddressEntities");

            migrationBuilder.RenameTable(
                name: "AddressEntities",
                newName: "Addresses");

            migrationBuilder.RenameIndex(
                name: "IX_AddressEntities_CountryId",
                table: "Addresses",
                newName: "IX_Addresses_CountryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Countries_CountryId",
                table: "Addresses",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Countries_CountryId",
                table: "Addresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses");

            migrationBuilder.RenameTable(
                name: "Addresses",
                newName: "AddressEntities");

            migrationBuilder.RenameIndex(
                name: "IX_Addresses_CountryId",
                table: "AddressEntities",
                newName: "IX_AddressEntities_CountryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AddressEntities",
                table: "AddressEntities",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AddressEntities_Countries_CountryId",
                table: "AddressEntities",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
