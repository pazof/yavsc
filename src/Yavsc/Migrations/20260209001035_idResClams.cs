using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class idResClams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityResourceClaim_IdentityResources_IdentityResourceId",
                table: "IdentityResourceClaim");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IdentityResourceClaim",
                table: "IdentityResourceClaim");

            migrationBuilder.RenameTable(
                name: "IdentityResourceClaim",
                newName: "IdentityResourceClaims");

            migrationBuilder.RenameIndex(
                name: "IX_IdentityResourceClaim_IdentityResourceId",
                table: "IdentityResourceClaims",
                newName: "IX_IdentityResourceClaims_IdentityResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdentityResourceClaims",
                table: "IdentityResourceClaims",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityResourceClaims_IdentityResources_IdentityResourceId",
                table: "IdentityResourceClaims",
                column: "IdentityResourceId",
                principalTable: "IdentityResources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityResourceClaims_IdentityResources_IdentityResourceId",
                table: "IdentityResourceClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IdentityResourceClaims",
                table: "IdentityResourceClaims");

            migrationBuilder.RenameTable(
                name: "IdentityResourceClaims",
                newName: "IdentityResourceClaim");

            migrationBuilder.RenameIndex(
                name: "IX_IdentityResourceClaims_IdentityResourceId",
                table: "IdentityResourceClaim",
                newName: "IX_IdentityResourceClaim_IdentityResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdentityResourceClaim",
                table: "IdentityResourceClaim",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityResourceClaim_IdentityResources_IdentityResourceId",
                table: "IdentityResourceClaim",
                column: "IdentityResourceId",
                principalTable: "IdentityResources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
