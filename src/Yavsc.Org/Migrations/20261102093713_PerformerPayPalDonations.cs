using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class PerformerPayPalDonations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DonationPayPalEmail",
                table: "Performers",
                type: "character varying(127)",
                maxLength: 127,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PerformerDonations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DonorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    PerformerId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    RecipientEmail = table.Column<string>(type: "character varying(127)", maxLength: 127, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    CheckoutToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransactionId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Revision = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformerDonations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformerDonations_AspNetUsers_DonorId",
                        column: x => x.DonorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerformerDonations_Performers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "Performers",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PerformerDonations_CheckoutToken",
                table: "PerformerDonations",
                column: "CheckoutToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformerDonations_DonorId",
                table: "PerformerDonations",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformerDonations_PerformerId",
                table: "PerformerDonations",
                column: "PerformerId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformerDonations_TransactionId",
                table: "PerformerDonations",
                column: "TransactionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerformerDonations");

            migrationBuilder.DropColumn(
                name: "DonationPayPalEmail",
                table: "Performers");
        }
    }
}
