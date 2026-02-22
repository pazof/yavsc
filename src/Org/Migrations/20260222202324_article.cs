using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class article : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Comment",
                newName: "Article");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "BlogSpot",
                newName: "Article");

            migrationBuilder.CreateIndex(
                name: "IX_MusicalPreference_TendencyId",
                table: "MusicalPreference",
                column: "TendencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_MusicalPreference_MusicalTendency_TendencyId",
                table: "MusicalPreference",
                column: "TendencyId",
                principalTable: "MusicalTendency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MusicalPreference_MusicalTendency_TendencyId",
                table: "MusicalPreference");

            migrationBuilder.DropIndex(
                name: "IX_MusicalPreference_TendencyId",
                table: "MusicalPreference");

            migrationBuilder.RenameColumn(
                name: "Article",
                table: "Comment",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "Article",
                table: "BlogSpot",
                newName: "Content");
        }
    }
}
