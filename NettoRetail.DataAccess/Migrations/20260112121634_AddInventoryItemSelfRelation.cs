using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NettoRetail.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryItemSelfRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
            ALTER TABLE "InventoryItems"
            ADD COLUMN IF NOT EXISTS "HasVariations" boolean NOT NULL DEFAULT FALSE;
            """);

            migrationBuilder.Sql("""
            ALTER TABLE "InventoryItems"
            ADD COLUMN IF NOT EXISTS "IsVariation" boolean NOT NULL DEFAULT FALSE;
            """);

            migrationBuilder.Sql("""
            ALTER TABLE "InventoryItems"
            ADD COLUMN IF NOT EXISTS "ParentItemId" uuid NULL;
            """);

            migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_InventoryItems_ParentItemId"
            ON "InventoryItems" ("ParentItemId");
            """);

            migrationBuilder.Sql("""
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1
                    FROM pg_constraint c
                    WHERE c.conname = 'FK_InventoryItems_InventoryItems_ParentItemId'
                ) THEN
                    ALTER TABLE "InventoryItems"
                    ADD CONSTRAINT "FK_InventoryItems_InventoryItems_ParentItemId"
                    FOREIGN KEY ("ParentItemId") REFERENCES "InventoryItems" ("Id")
                    ON DELETE RESTRICT;
                END IF;
            END $$;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM pg_constraint c
                    WHERE c.conname = 'FK_InventoryItems_InventoryItems_ParentItemId'
                ) THEN
                    ALTER TABLE "InventoryItems"
                    DROP CONSTRAINT "FK_InventoryItems_InventoryItems_ParentItemId";
                END IF;
            END $$;
            """);

            migrationBuilder.Sql("""
            DROP INDEX IF EXISTS "IX_InventoryItems_ParentItemId";
            """);

            migrationBuilder.Sql("""
            ALTER TABLE "InventoryItems" DROP COLUMN IF EXISTS "HasVariations";
            """);

            migrationBuilder.Sql("""
            ALTER TABLE "InventoryItems" DROP COLUMN IF EXISTS "IsVariation";
            """);

            migrationBuilder.Sql("""
            ALTER TABLE "InventoryItems" DROP COLUMN IF EXISTS "ParentItemId";
            """);
        }
    }
}
