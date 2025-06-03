using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SectionTypeMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SectionType",
                table: "Contents",
                newName: "SectionType_Old");

            migrationBuilder.AddColumn<int>(
                name: "SectionType",
                table: "Contents",
                type: "integer",
                nullable: false,
                defaultValue: 0); // temp default; will be overwritten below

            migrationBuilder.Sql(@"
                DELETE FROM ""Contents""
                WHERE ""SectionType_Old"" NOT IN (
                    'OurShop',
                    'OurStory',
                    'TermsAndConditions',
                    'PrivacyPolicy',
                    'CookiesPolicy'
                );
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Contents"" SET ""SectionType"" =
                    CASE ""SectionType_Old""
                        WHEN 'OurShop' THEN 0
                        WHEN 'OurStory' THEN 1
                        WHEN 'TermsAndConditions' THEN 2
                        WHEN 'PrivacyPolicy' THEN 3
                        WHEN 'CookiesPolicy' THEN 4
                    END;
            ");

            migrationBuilder.DropColumn(
                name: "SectionType_Old",
                table: "Contents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SectionType_Old",
                table: "Contents",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Contents"" SET ""SectionType_Old"" =
                    CASE ""SectionType""
                        WHEN 0 THEN 'OurShop'
                        WHEN 1 THEN 'OurStory'
                        WHEN 2 THEN 'TermsAndConditions'
                        WHEN 3 THEN 'PrivacyPolicy'
                        WHEN 4 THEN 'CookiesPolicy'
                    END;
            ");

            migrationBuilder.DropColumn(
                name: "SectionType",
                table: "Contents");

            migrationBuilder.RenameColumn(
                name: "SectionType_Old",
                table: "Contents",
                newName: "SectionType");
        }
    }
}
