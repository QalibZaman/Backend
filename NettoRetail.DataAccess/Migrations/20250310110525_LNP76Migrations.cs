using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class LNP76Migrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Roles_RoleId",
                table: "Staff");

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
                name: "IX_Countries_Name",
                table: "Countries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_PhoneCode",
                table: "Countries",
                column: "PhoneCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Shops_Countries_CountryId",
                table: "Shops",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_Shops_Countries_CountryId",
                table: "Shops");

            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Roles_RoleId",
                table: "Staff");

            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Shops_ShopId",
                table: "Staff");

            migrationBuilder.DropTable(
                name: "Countries");

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
