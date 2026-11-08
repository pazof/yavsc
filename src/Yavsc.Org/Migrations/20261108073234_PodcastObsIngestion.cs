using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class PodcastObsIngestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ObsConnectionId",
                table: "LiveFlow",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ObsEnabledUntil",
                table: "LiveFlow",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObsKeyHash",
                table: "LiveFlow",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObsLastError",
                table: "LiveFlow",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObsConnectionId",
                table: "LiveFlow");

            migrationBuilder.DropColumn(
                name: "ObsEnabledUntil",
                table: "LiveFlow");

            migrationBuilder.DropColumn(
                name: "ObsKeyHash",
                table: "LiveFlow");

            migrationBuilder.DropColumn(
                name: "ObsLastError",
                table: "LiveFlow");
        }
    }
}
