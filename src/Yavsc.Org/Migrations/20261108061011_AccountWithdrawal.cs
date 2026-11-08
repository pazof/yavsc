using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class AccountWithdrawal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(name: "AK_AspNetUsers_Email", table: "AspNetUsers");
            migrationBuilder.CreateIndex(name: "IX_AspNetUsers_Email", table: "AspNetUsers", column: "Email", unique: true);
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "WithdrawalRequestedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "WithdrawnAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_AspNetUsers_Email", table: "AspNetUsers");
            migrationBuilder.AddUniqueConstraint(name: "AK_AspNetUsers_Email", table: "AspNetUsers", column: "Email");
            migrationBuilder.DropColumn(
                name: "WithdrawalRequestedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WithdrawnAt",
                table: "AspNetUsers");
        }
    }
}
