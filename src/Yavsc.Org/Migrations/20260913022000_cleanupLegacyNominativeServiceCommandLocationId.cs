using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Yavsc.Models;

#nullable disable

namespace Yavsc.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260913022000_cleanupLegacyNominativeServiceCommandLocationId")]
    public partial class cleanupLegacyNominativeServiceCommandLocationId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
                        migrationBuilder.Sql(@"
UPDATE ""NominativeServiceCommand""
SET ""RdvQuery_LocationId"" = COALESCE(""RdvQuery_LocationId"", ""LocationId"")
WHERE ""Discriminator"" = 'RdvQuery'
    AND ""LocationId"" IS NOT NULL;
");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommand_Locations_LocationId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropIndex(
                name: "IX_NominativeServiceCommand_LocationId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "NominativeServiceCommand");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LocationId",
                table: "NominativeServiceCommand",
                type: "bigint",
                nullable: true);

                        migrationBuilder.Sql(@"
UPDATE ""NominativeServiceCommand""
SET ""LocationId"" = ""RdvQuery_LocationId""
WHERE ""Discriminator"" = 'RdvQuery'
    AND ""RdvQuery_LocationId"" IS NOT NULL;
");

            migrationBuilder.CreateIndex(
                name: "IX_NominativeServiceCommand_LocationId",
                table: "NominativeServiceCommand",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommand_Locations_LocationId",
                table: "NominativeServiceCommand",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id");
        }
    }
}