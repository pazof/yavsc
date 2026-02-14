using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class ApiResourceProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiResourceProperty_ApiResources_ApiResourceId",
                table: "ApiResourceProperty");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiResourceProperty",
                table: "ApiResourceProperty");

            migrationBuilder.RenameTable(
                name: "ApiResourceProperty",
                newName: "ApiResourceProperties");

            migrationBuilder.RenameIndex(
                name: "IX_ApiResourceProperty_ApiResourceId",
                table: "ApiResourceProperties",
                newName: "IX_ApiResourceProperties_ApiResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiResourceProperties",
                table: "ApiResourceProperties",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiResourceProperties_ApiResources_ApiResourceId",
                table: "ApiResourceProperties",
                column: "ApiResourceId",
                principalTable: "ApiResources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiResourceProperties_ApiResources_ApiResourceId",
                table: "ApiResourceProperties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiResourceProperties",
                table: "ApiResourceProperties");

            migrationBuilder.RenameTable(
                name: "ApiResourceProperties",
                newName: "ApiResourceProperty");

            migrationBuilder.RenameIndex(
                name: "IX_ApiResourceProperties_ApiResourceId",
                table: "ApiResourceProperty",
                newName: "IX_ApiResourceProperty_ApiResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiResourceProperty",
                table: "ApiResourceProperty",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiResourceProperty_ApiResources_ApiResourceId",
                table: "ApiResourceProperty",
                column: "ApiResourceId",
                principalTable: "ApiResources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
