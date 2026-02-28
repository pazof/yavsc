using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class ApiResourceScopes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiResourceScope_ApiResources_ApiResourceId",
                table: "ApiResourceScope");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiResourceScope",
                table: "ApiResourceScope");

            migrationBuilder.RenameTable(
                name: "ApiResourceScope",
                newName: "ApiResourceScopes");

            migrationBuilder.RenameIndex(
                name: "IX_ApiResourceScope_ApiResourceId",
                table: "ApiResourceScopes",
                newName: "IX_ApiResourceScopes_ApiResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiResourceScopes",
                table: "ApiResourceScopes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiResourceScopes_ApiResources_ApiResourceId",
                table: "ApiResourceScopes",
                column: "ApiResourceId",
                principalTable: "ApiResources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiResourceScopes_ApiResources_ApiResourceId",
                table: "ApiResourceScopes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiResourceScopes",
                table: "ApiResourceScopes");

            migrationBuilder.RenameTable(
                name: "ApiResourceScopes",
                newName: "ApiResourceScope");

            migrationBuilder.RenameIndex(
                name: "IX_ApiResourceScopes_ApiResourceId",
                table: "ApiResourceScope",
                newName: "IX_ApiResourceScope_ApiResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiResourceScope",
                table: "ApiResourceScope",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiResourceScope_ApiResources_ApiResourceId",
                table: "ApiResourceScope",
                column: "ApiResourceId",
                principalTable: "ApiResources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
