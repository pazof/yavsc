using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class commentParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Blogspot_PostId",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_PostId",
                table: "Comment");


            migrationBuilder.CreateIndex(
                name: "IX_Comment_ReceiverId",
                table: "Comment",
                column: "ReceiverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Blogspot_ReceiverId",
                table: "Comment",
                column: "ReceiverId",
                principalTable: "Blogspot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Blogspot_ReceiverId",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_ReceiverId",
                table: "Comment");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_PostId",
                table: "Comment",
                column: "PostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Blogspot_PostId",
                table: "Comment",
                column: "PostId",
                principalTable: "Blogspot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
