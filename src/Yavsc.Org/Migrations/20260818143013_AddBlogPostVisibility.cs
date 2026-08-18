using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogPostVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientGrantTypes_Clients_ClientId1",
                table: "ClientGrantTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientRedirectUris_Clients_ClientId1",
                table: "ClientRedirectUris");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientScopes_Clients_ClientId1",
                table: "ClientScopes");

            migrationBuilder.DropIndex(
                name: "IX_ClientScopes_ClientId1",
                table: "ClientScopes");

            migrationBuilder.DropIndex(
                name: "IX_ClientRedirectUris_ClientId1",
                table: "ClientRedirectUris");

            migrationBuilder.DropIndex(
                name: "IX_ClientGrantTypes_ClientId1",
                table: "ClientGrantTypes");

            migrationBuilder.DropColumn(
                name: "ClientId1",
                table: "ClientScopes");

            migrationBuilder.DropColumn(
                name: "ClientId1",
                table: "ClientRedirectUris");

            migrationBuilder.DropColumn(
                name: "ClientId1",
                table: "ClientGrantTypes");

            migrationBuilder.AddColumn<int>(
                name: "Visibility",
                table: "BlogSpot",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "BlogSpot");

            migrationBuilder.AddColumn<int>(
                name: "ClientId1",
                table: "ClientScopes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClientId1",
                table: "ClientRedirectUris",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClientId1",
                table: "ClientGrantTypes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientScopes_ClientId1",
                table: "ClientScopes",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRedirectUris_ClientId1",
                table: "ClientRedirectUris",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_ClientGrantTypes_ClientId1",
                table: "ClientGrantTypes",
                column: "ClientId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientGrantTypes_Clients_ClientId1",
                table: "ClientGrantTypes",
                column: "ClientId1",
                principalTable: "Clients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRedirectUris_Clients_ClientId1",
                table: "ClientRedirectUris",
                column: "ClientId1",
                principalTable: "Clients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientScopes_Clients_ClientId1",
                table: "ClientScopes",
                column: "ClientId1",
                principalTable: "Clients",
                principalColumn: "Id");
        }
    }
}
