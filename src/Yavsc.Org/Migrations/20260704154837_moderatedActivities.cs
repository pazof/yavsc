using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class moderatedActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MusicalPreference_GeneralSettings_GeneralSettingsUserId",
                table: "MusicalPreference");

            migrationBuilder.RenameColumn(
                name: "GeneralSettingsUserId",
                table: "MusicalPreference",
                newName: "MusicLoverSettingsUserId");

            migrationBuilder.RenameIndex(
                name: "IX_MusicalPreference_GeneralSettingsUserId",
                table: "MusicalPreference",
                newName: "IX_MusicalPreference_MusicLoverSettingsUserId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Bug",
                type: "character varying(10240)",
                maxLength: 10240,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Moderated",
                table: "Activities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Signatures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstimateId = table.Column<long>(type: "bigint", nullable: false),
                    SignerId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CoordinateMax = table.Column<int>(type: "integer", nullable: false, defaultValue: 10000),
                    Strokes = table.Column<int[]>(type: "integer[]", nullable: false),
                    CapturedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Signatures_AspNetUsers_SignerId",
                        column: x => x.SignerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Signatures_Estimates_EstimateId",
                        column: x => x.EstimateId,
                        principalTable: "Estimates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Signatures_EstimateId_Type",
                table: "Signatures",
                columns: new[] { "EstimateId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Signatures_SignerId",
                table: "Signatures",
                column: "SignerId");

            migrationBuilder.AddForeignKey(
                name: "FK_MusicalPreference_GeneralSettings_MusicLoverSettingsUserId",
                table: "MusicalPreference",
                column: "MusicLoverSettingsUserId",
                principalTable: "GeneralSettings",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MusicalPreference_GeneralSettings_MusicLoverSettingsUserId",
                table: "MusicalPreference");

            migrationBuilder.DropTable(
                name: "Signatures");

            migrationBuilder.DropColumn(
                name: "Moderated",
                table: "Activities");

            migrationBuilder.RenameColumn(
                name: "MusicLoverSettingsUserId",
                table: "MusicalPreference",
                newName: "GeneralSettingsUserId");

            migrationBuilder.RenameIndex(
                name: "IX_MusicalPreference_MusicLoverSettingsUserId",
                table: "MusicalPreference",
                newName: "IX_MusicalPreference_GeneralSettingsUserId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Bug",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10240)",
                oldMaxLength: 10240,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicalPreference_GeneralSettings_GeneralSettingsUserId",
                table: "MusicalPreference",
                column: "GeneralSettingsUserId",
                principalTable: "GeneralSettings",
                principalColumn: "UserId");
        }
    }
}
