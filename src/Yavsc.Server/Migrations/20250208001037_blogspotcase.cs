using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class blogspotcase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blogspot_AspNetUsers_AuthorId",
                table: "Blogspot");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogTag_Blogspot_PostId",
                table: "BlogTag");

            migrationBuilder.DropForeignKey(
                name: "FK_CircleAuthorizationToBlogPost_Blogspot_BlogPostId",
                table: "CircleAuthorizationToBlogPost");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Blogspot_ReceiverId",
                table: "Comment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Blogspot",
                table: "Blogspot");

            migrationBuilder.DropColumn(
                name: "Rate",
                table: "Blogspot");

            migrationBuilder.RenameTable(
                name: "Blogspot",
                newName: "BlogSpot");

            migrationBuilder.RenameIndex(
                name: "IX_Blogspot_AuthorId",
                table: "BlogSpot",
                newName: "IX_BlogSpot_AuthorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogSpot",
                table: "BlogSpot",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogSpot_AspNetUsers_AuthorId",
                table: "BlogSpot",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogTag_BlogSpot_PostId",
                table: "BlogTag",
                column: "PostId",
                principalTable: "BlogSpot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CircleAuthorizationToBlogPost_BlogSpot_BlogPostId",
                table: "CircleAuthorizationToBlogPost",
                column: "BlogPostId",
                principalTable: "BlogSpot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_BlogSpot_ReceiverId",
                table: "Comment",
                column: "ReceiverId",
                principalTable: "BlogSpot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogSpot_AspNetUsers_AuthorId",
                table: "BlogSpot");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogTag_BlogSpot_PostId",
                table: "BlogTag");

            migrationBuilder.DropForeignKey(
                name: "FK_CircleAuthorizationToBlogPost_BlogSpot_BlogPostId",
                table: "CircleAuthorizationToBlogPost");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_BlogSpot_ReceiverId",
                table: "Comment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogSpot",
                table: "BlogSpot");

            migrationBuilder.RenameTable(
                name: "BlogSpot",
                newName: "Blogspot");

            migrationBuilder.RenameIndex(
                name: "IX_BlogSpot_AuthorId",
                table: "Blogspot",
                newName: "IX_Blogspot_AuthorId");

            migrationBuilder.AddColumn<int>(
                name: "Rate",
                table: "Blogspot",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Blogspot",
                table: "Blogspot",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Blogspot_AspNetUsers_AuthorId",
                table: "Blogspot",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogTag_Blogspot_PostId",
                table: "BlogTag",
                column: "PostId",
                principalTable: "Blogspot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CircleAuthorizationToBlogPost_Blogspot_BlogPostId",
                table: "CircleAuthorizationToBlogPost",
                column: "BlogPostId",
                principalTable: "Blogspot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Blogspot_ReceiverId",
                table: "Comment",
                column: "ReceiverId",
                principalTable: "Blogspot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
