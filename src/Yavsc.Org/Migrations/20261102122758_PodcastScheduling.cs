using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class PodcastScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlannedDurationMinutes",
                table: "LiveFlow",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PodcastDeleted",
                table: "LiveFlow",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PodcastId",
                table: "LiveFlow",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PreparedAt",
                table: "LiveFlow",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicAnnouncement",
                table: "LiveFlow",
                type: "character varying(1023)",
                maxLength: 1023,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ScheduledStartAt",
                table: "LiveFlow",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartedAt",
                table: "LiveFlow",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LiveFlow_PodcastId",
                table: "LiveFlow",
                column: "PodcastId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LiveFlow_PodcastId",
                table: "LiveFlow");

            migrationBuilder.DropColumn(
                name: "PlannedDurationMinutes",
                table: "LiveFlow");

            migrationBuilder.DropColumn(
                name: "PodcastDeleted",
                table: "LiveFlow");

            migrationBuilder.DropColumn(
                name: "PodcastId",
                table: "LiveFlow");

            migrationBuilder.DropColumn(
                name: "PreparedAt",
                table: "LiveFlow");

            migrationBuilder.DropColumn(
                name: "PublicAnnouncement",
                table: "LiveFlow");

            migrationBuilder.DropColumn(
                name: "ScheduledStartAt",
                table: "LiveFlow");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "LiveFlow");
        }
    }
}
