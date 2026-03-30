using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class NonOptionalPasswordUpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                    UPDATE "Users"
                    SET "PasswordUpdatedAt" = "DateCreated"
                    WHERE "PasswordUpdatedAt" IS NULL
                """);

            migrationBuilder.Sql(
                """
                    CREATE OR REPLACE FUNCTION update_password_timestamp()
                    RETURNS TRIGGER AS $$
                        BEGIN
                            IF(TG_OP = 'INSERT' OR NEW."PasswordHash" IS DISTINCT FROM OLD."PasswordHash") THEN
                                NEW."PasswordUpdatedAt" = NOW();
                            END IF;

                            RETURN NEW;
                        END;
                    $$ LANGUAGE plpgsql;

                    DROP TRIGGER IF EXISTS trigger_update_password_timestamp ON "Users";

                    CREATE TRIGGER trigger_update_password_timestamp
                    BEFORE UPDATE OR INSERT ON "Users"
                    FOR EACH ROW
                    EXECUTE FUNCTION update_password_timestamp();
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PasswordUpdatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                    DROP TRIGGER IF EXISTS trigger_update_password_timestamp ON "Users";

                    DROP FUNCTION IF EXISTS update_password_timestamp();
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PasswordUpdatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}
