using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class ApiScopeProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiScopeProperty_ApiScopes_ScopeId",
                table: "ApiScopeProperty");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiScopeProperty",
                table: "ApiScopeProperty");

            migrationBuilder.RenameTable(
                name: "ApiScopeProperty",
                newName: "ApiScopeProperties");

            migrationBuilder.RenameIndex(
                name: "IX_ApiScopeProperty_ScopeId",
                table: "ApiScopeProperties",
                newName: "IX_ApiScopeProperties_ScopeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiScopeProperties",
                table: "ApiScopeProperties",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiScopeProperties_ApiScopes_ScopeId",
                table: "ApiScopeProperties",
                column: "ScopeId",
                principalTable: "ApiScopes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiScopeProperties_ApiScopes_ScopeId",
                table: "ApiScopeProperties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiScopeProperties",
                table: "ApiScopeProperties");

            migrationBuilder.RenameTable(
                name: "ApiScopeProperties",
                newName: "ApiScopeProperty");

            migrationBuilder.RenameIndex(
                name: "IX_ApiScopeProperties_ScopeId",
                table: "ApiScopeProperty",
                newName: "IX_ApiScopeProperty_ScopeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiScopeProperty",
                table: "ApiScopeProperty",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiScopeProperty_ApiScopes_ScopeId",
                table: "ApiScopeProperty",
                column: "ScopeId",
                principalTable: "ApiScopes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
