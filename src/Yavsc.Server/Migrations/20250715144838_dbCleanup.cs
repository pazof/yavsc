using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class dbCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankBook_BankStatus_BalanceId",
                table: "BankBook");

            migrationBuilder.DropTable(
                name: "OAuth2Tokens");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BankBook",
                table: "BankBook");

            migrationBuilder.RenameTable(
                name: "BankBook",
                newName: "BalanceImpact");

            migrationBuilder.RenameIndex(
                name: "IX_BankBook_BalanceId",
                table: "BalanceImpact",
                newName: "IX_BalanceImpact_BalanceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BalanceImpact",
                table: "BalanceImpact",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BalanceImpact_BankStatus_BalanceId",
                table: "BalanceImpact",
                column: "BalanceId",
                principalTable: "BankStatus",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BalanceImpact_BankStatus_BalanceId",
                table: "BalanceImpact");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BalanceImpact",
                table: "BalanceImpact");

            migrationBuilder.RenameTable(
                name: "BalanceImpact",
                newName: "BankBook");

            migrationBuilder.RenameIndex(
                name: "IX_BalanceImpact_BalanceId",
                table: "BankBook",
                newName: "IX_BankBook_BalanceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BankBook",
                table: "BankBook",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "OAuth2Tokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    AccessToken = table.Column<string>(type: "text", nullable: false),
                    Expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresIn = table.Column<string>(type: "text", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: false),
                    TokenType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuth2Tokens", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ClientId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExpiresUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IssuedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProtectedTicket = table.Column<string>(type: "text", nullable: false),
                    Subject = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_BankBook_BankStatus_BalanceId",
                table: "BankBook",
                column: "BalanceId",
                principalTable: "BankStatus",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
