using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class blogPusb2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_blogspotPublications_BlogSpot_BlogpostId",
                table: "blogspotPublications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_blogspotPublications",
                table: "blogspotPublications");

            migrationBuilder.RenameTable(
                name: "blogspotPublications",
                newName: "blogSpotPublications");

            migrationBuilder.AddColumn<bool>(
                name: "Publish",
                table: "BlogSpot",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_blogSpotPublications",
                table: "blogSpotPublications",
                column: "BlogpostId");

            migrationBuilder.AddForeignKey(
                name: "FK_blogSpotPublications_BlogSpot_BlogpostId",
                table: "blogSpotPublications",
                column: "BlogpostId",
                principalTable: "BlogSpot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_blogSpotPublications_BlogSpot_BlogpostId",
                table: "blogSpotPublications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_blogSpotPublications",
                table: "blogSpotPublications");

            migrationBuilder.DropColumn(
                name: "Publish",
                table: "BlogSpot");

            migrationBuilder.RenameTable(
                name: "blogSpotPublications",
                newName: "blogspotPublications");

            migrationBuilder.AddPrimaryKey(
                name: "PK_blogspotPublications",
                table: "blogspotPublications",
                column: "BlogpostId");

            migrationBuilder.AddForeignKey(
                name: "FK_blogspotPublications_BlogSpot_BlogpostId",
                table: "blogspotPublications",
                column: "BlogpostId",
                principalTable: "BlogSpot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
