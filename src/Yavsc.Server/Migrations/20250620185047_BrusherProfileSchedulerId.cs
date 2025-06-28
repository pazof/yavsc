using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class BrusherProfileSchedulerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BrusherProfile_Schedule_ScheduleOwnerId",
                table: "BrusherProfile");

            migrationBuilder.AlterColumn<string>(
                name: "ScheduleOwnerId",
                table: "BrusherProfile",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_BrusherProfile_Schedule_ScheduleOwnerId",
                table: "BrusherProfile",
                column: "ScheduleOwnerId",
                principalTable: "Schedule",
                principalColumn: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BrusherProfile_Schedule_ScheduleOwnerId",
                table: "BrusherProfile");

            migrationBuilder.AlterColumn<string>(
                name: "ScheduleOwnerId",
                table: "BrusherProfile",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BrusherProfile_Schedule_ScheduleOwnerId",
                table: "BrusherProfile",
                column: "ScheduleOwnerId",
                principalTable: "Schedule",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
