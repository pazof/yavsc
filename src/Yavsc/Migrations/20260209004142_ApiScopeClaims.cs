using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class ApiScopeClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiScopeClaim_ApiScopes_ScopeId",
                table: "ApiScopeClaim");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiScopeClaim",
                table: "ApiScopeClaim");

            migrationBuilder.RenameTable(
                name: "ApiScopeClaim",
                newName: "ApiScopeClaims");

            migrationBuilder.RenameIndex(
                name: "IX_ApiScopeClaim_ScopeId",
                table: "ApiScopeClaims",
                newName: "IX_ApiScopeClaims_ScopeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiScopeClaims",
                table: "ApiScopeClaims",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiScopeClaims_ApiScopes_ScopeId",
                table: "ApiScopeClaims",
                column: "ScopeId",
                principalTable: "ApiScopes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiScopeClaims_ApiScopes_ScopeId",
                table: "ApiScopeClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiScopeClaims",
                table: "ApiScopeClaims");

            migrationBuilder.RenameTable(
                name: "ApiScopeClaims",
                newName: "ApiScopeClaim");

            migrationBuilder.RenameIndex(
                name: "IX_ApiScopeClaims_ScopeId",
                table: "ApiScopeClaim",
                newName: "IX_ApiScopeClaim_ScopeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiScopeClaim",
                table: "ApiScopeClaim",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiScopeClaim_ApiScopes_ScopeId",
                table: "ApiScopeClaim",
                column: "ScopeId",
                principalTable: "ApiScopes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
