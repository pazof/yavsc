using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class blogPostId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_blogSpotPublications_BlogSpot_BlogpostId",
                table: "blogSpotPublications");

            migrationBuilder.RenameColumn(
                name: "BlogpostId",
                table: "blogSpotPublications",
                newName: "PostId");

            migrationBuilder.AddForeignKey(
                name: "FK_blogSpotPublications_BlogSpot_PostId",
                table: "blogSpotPublications",
                column: "PostId",
                principalTable: "BlogSpot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_blogSpotPublications_BlogSpot_PostId",
                table: "blogSpotPublications");

            migrationBuilder.RenameColumn(
                name: "PostId",
                table: "blogSpotPublications",
                newName: "BlogpostId");

            migrationBuilder.AddForeignKey(
                name: "FK_blogSpotPublications_BlogSpot_BlogpostId",
                table: "blogSpotPublications",
                column: "BlogpostId",
                principalTable: "BlogSpot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
