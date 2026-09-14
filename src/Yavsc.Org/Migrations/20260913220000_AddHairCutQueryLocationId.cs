using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Yavsc.Models;

#nullable disable

namespace Yavsc.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260913220000_AddHairCutQueryLocationId")]
    public partial class AddHairCutQueryLocationId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LocationId",
                table: "NominativeServiceCommands",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NominativeServiceCommands_LocationId",
                table: "NominativeServiceCommands",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommands_Locations_LocationId",
                table: "NominativeServiceCommands",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommands_Locations_LocationId",
                table: "NominativeServiceCommands");

            migrationBuilder.DropIndex(
                name: "IX_NominativeServiceCommands_LocationId",
                table: "NominativeServiceCommands");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "NominativeServiceCommands");
        }
    }
}
