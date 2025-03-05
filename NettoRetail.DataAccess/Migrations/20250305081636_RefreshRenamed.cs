using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RefreshRenamed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Refresh",
                table: "Sessions",
                newName: "RefreshToken");

            migrationBuilder.RenameIndex(
                name: "IX_Sessions_Refresh",
                table: "Sessions",
                newName: "IX_Sessions_RefreshToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "Sessions",
                newName: "Refresh");

            migrationBuilder.RenameIndex(
                name: "IX_Sessions_RefreshToken",
                table: "Sessions",
                newName: "IX_Sessions_Refresh");
        }
    }
}
