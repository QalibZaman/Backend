using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedPhoneMasksOfCountryRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 2,
                column: "PhoneMask",
                value: "^\\d{3} \\d{3} \\d{3}$");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 9,
                column: "PhoneMask",
                value: "^\\d{2} \\d{3} \\d{4}$");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 14,
                column: "PhoneMask",
                value: "^\\d{2} \\d{3} \\d{4}$");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 18,
                column: "PhoneMask",
                value: "^\\d{3} \\d{3} \\d{3}$");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 25,
                column: "PhoneMask",
                value: "^\\d{2} \\d{3} \\d{3}$");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 27,
                column: "PhoneMask",
                value: "^\\d{2} \\d{3} \\d{4}$");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 2,
                column: "PhoneMask",
                value: "^\\d \\d{3} \\d{2} \\d{2}$");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 9,
                column: "PhoneMask",
                value: "^\\d \\d{3} \\d{4}$");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 14,
                column: "PhoneMask",
                value: "^\\d \\d{3} \\d{4}$");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 18,
                column: "PhoneMask",
                value: "^\\d{3} \\d{3}$");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 25,
                column: "PhoneMask",
                value: "^\\d \\d{4} \\d{4}$");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 27,
                column: "PhoneMask",
                value: "^\\d{3} \\d{3} \\d{4}$");
        }
    }
}
