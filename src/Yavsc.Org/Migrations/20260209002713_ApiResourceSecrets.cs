using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class ApiResourceSecrets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiResourceSecret_ApiResources_ApiResourceId",
                table: "ApiResourceSecret");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiResourceSecret",
                table: "ApiResourceSecret");

            migrationBuilder.RenameTable(
                name: "ApiResourceSecret",
                newName: "ApiResourceSecrets");

            migrationBuilder.RenameIndex(
                name: "IX_ApiResourceSecret_ApiResourceId",
                table: "ApiResourceSecrets",
                newName: "IX_ApiResourceSecrets_ApiResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiResourceSecrets",
                table: "ApiResourceSecrets",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiResourceSecrets_ApiResources_ApiResourceId",
                table: "ApiResourceSecrets",
                column: "ApiResourceId",
                principalTable: "ApiResources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiResourceSecrets_ApiResources_ApiResourceId",
                table: "ApiResourceSecrets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiResourceSecrets",
                table: "ApiResourceSecrets");

            migrationBuilder.RenameTable(
                name: "ApiResourceSecrets",
                newName: "ApiResourceSecret");

            migrationBuilder.RenameIndex(
                name: "IX_ApiResourceSecrets_ApiResourceId",
                table: "ApiResourceSecret",
                newName: "IX_ApiResourceSecret_ApiResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiResourceSecret",
                table: "ApiResourceSecret",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiResourceSecret_ApiResources_ApiResourceId",
                table: "ApiResourceSecret",
                column: "ApiResourceId",
                principalTable: "ApiResources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
