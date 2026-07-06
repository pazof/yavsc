using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class activityModerated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MusicalPreference_GeneralSettings_GeneralSettingsUserId",
                table: "MusicalPreference");

            migrationBuilder.DropTable(
                name: "GeneralSettings");

            migrationBuilder.DropColumn(
                name: "Accepted",
                table: "RdvQueries");

            migrationBuilder.DropColumn(
                name: "Decided",
                table: "RdvQueries");

            migrationBuilder.DropColumn(
                name: "Accepted",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "Decided",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "Accepted",
                table: "HairMultiCutQueries");

            migrationBuilder.DropColumn(
                name: "Decided",
                table: "HairMultiCutQueries");

            migrationBuilder.DropColumn(
                name: "Accepted",
                table: "HairCutQueries");

            migrationBuilder.DropColumn(
                name: "Decided",
                table: "HairCutQueries");

            migrationBuilder.RenameColumn(
                name: "Previsional",
                table: "RdvQueries",
                newName: "Provisional");

            migrationBuilder.RenameColumn(
                name: "Previsional",
                table: "Project",
                newName: "Provisional");

            migrationBuilder.RenameColumn(
                name: "GeneralSettingsUserId",
                table: "MusicalPreference",
                newName: "MusicLoverSettingsUserId");

            migrationBuilder.RenameIndex(
                name: "IX_MusicalPreference_GeneralSettingsUserId",
                table: "MusicalPreference",
                newName: "IX_MusicalPreference_MusicLoverSettingsUserId");

            migrationBuilder.RenameColumn(
                name: "Previsional",
                table: "HairMultiCutQueries",
                newName: "Provisional");

            migrationBuilder.RenameColumn(
                name: "Previsional",
                table: "HairCutQueries",
                newName: "Provisional");

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
                name: "MusicLoverSettings",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicLoverSettings", x => x.UserId);
                });

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
                name: "FK_MusicalPreference_MusicLoverSettings_MusicLoverSettingsUser~",
                table: "MusicalPreference",
                column: "MusicLoverSettingsUserId",
                principalTable: "MusicLoverSettings",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MusicalPreference_MusicLoverSettings_MusicLoverSettingsUser~",
                table: "MusicalPreference");

            migrationBuilder.DropTable(
                name: "MusicLoverSettings");

            migrationBuilder.DropTable(
                name: "Signatures");

            migrationBuilder.DropColumn(
                name: "Moderated",
                table: "Activities");

            migrationBuilder.RenameColumn(
                name: "Provisional",
                table: "RdvQueries",
                newName: "Previsional");

            migrationBuilder.RenameColumn(
                name: "Provisional",
                table: "Project",
                newName: "Previsional");

            migrationBuilder.RenameColumn(
                name: "MusicLoverSettingsUserId",
                table: "MusicalPreference",
                newName: "GeneralSettingsUserId");

            migrationBuilder.RenameIndex(
                name: "IX_MusicalPreference_MusicLoverSettingsUserId",
                table: "MusicalPreference",
                newName: "IX_MusicalPreference_GeneralSettingsUserId");

            migrationBuilder.RenameColumn(
                name: "Provisional",
                table: "HairMultiCutQueries",
                newName: "Previsional");

            migrationBuilder.RenameColumn(
                name: "Provisional",
                table: "HairCutQueries",
                newName: "Previsional");

            migrationBuilder.AddColumn<bool>(
                name: "Accepted",
                table: "RdvQueries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Decided",
                table: "RdvQueries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Accepted",
                table: "Project",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Decided",
                table: "Project",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Accepted",
                table: "HairMultiCutQueries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Decided",
                table: "HairMultiCutQueries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Accepted",
                table: "HairCutQueries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Decided",
                table: "HairCutQueries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Bug",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10240)",
                oldMaxLength: 10240,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "GeneralSettings",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralSettings", x => x.UserId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_MusicalPreference_GeneralSettings_GeneralSettingsUserId",
                table: "MusicalPreference",
                column: "GeneralSettingsUserId",
                principalTable: "GeneralSettings",
                principalColumn: "UserId");
        }
    }
}
