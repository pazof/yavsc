using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class IdentityResourceProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityResourceProperty_IdentityResources_IdentityResource~",
                table: "IdentityResourceProperty");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IdentityResourceProperty",
                table: "IdentityResourceProperty");

            migrationBuilder.RenameTable(
                name: "IdentityResourceProperty",
                newName: "IdentityResourceProperties");

            migrationBuilder.RenameIndex(
                name: "IX_IdentityResourceProperty_IdentityResourceId",
                table: "IdentityResourceProperties",
                newName: "IX_IdentityResourceProperties_IdentityResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdentityResourceProperties",
                table: "IdentityResourceProperties",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityResourceProperties_IdentityResources_IdentityResour~",
                table: "IdentityResourceProperties",
                column: "IdentityResourceId",
                principalTable: "IdentityResources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityResourceProperties_IdentityResources_IdentityResour~",
                table: "IdentityResourceProperties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IdentityResourceProperties",
                table: "IdentityResourceProperties");

            migrationBuilder.RenameTable(
                name: "IdentityResourceProperties",
                newName: "IdentityResourceProperty");

            migrationBuilder.RenameIndex(
                name: "IX_IdentityResourceProperties_IdentityResourceId",
                table: "IdentityResourceProperty",
                newName: "IX_IdentityResourceProperty_IdentityResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdentityResourceProperty",
                table: "IdentityResourceProperty",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityResourceProperty_IdentityResources_IdentityResource~",
                table: "IdentityResourceProperty",
                column: "IdentityResourceId",
                principalTable: "IdentityResources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
