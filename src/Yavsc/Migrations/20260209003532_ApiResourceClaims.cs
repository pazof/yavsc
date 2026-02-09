using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class ApiResourceClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiResourceClaim_ApiResources_ApiResourceId",
                table: "ApiResourceClaim");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiResourceClaim",
                table: "ApiResourceClaim");

            migrationBuilder.RenameTable(
                name: "ApiResourceClaim",
                newName: "ApiResourceClaims");

            migrationBuilder.RenameIndex(
                name: "IX_ApiResourceClaim_ApiResourceId",
                table: "ApiResourceClaims",
                newName: "IX_ApiResourceClaims_ApiResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiResourceClaims",
                table: "ApiResourceClaims",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiResourceClaims_ApiResources_ApiResourceId",
                table: "ApiResourceClaims",
                column: "ApiResourceId",
                principalTable: "ApiResources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiResourceClaims_ApiResources_ApiResourceId",
                table: "ApiResourceClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiResourceClaims",
                table: "ApiResourceClaims");

            migrationBuilder.RenameTable(
                name: "ApiResourceClaims",
                newName: "ApiResourceClaim");

            migrationBuilder.RenameIndex(
                name: "IX_ApiResourceClaims_ApiResourceId",
                table: "ApiResourceClaim",
                newName: "IX_ApiResourceClaim_ApiResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiResourceClaim",
                table: "ApiResourceClaim",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiResourceClaim_ApiResources_ApiResourceId",
                table: "ApiResourceClaim",
                column: "ApiResourceId",
                principalTable: "ApiResources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
