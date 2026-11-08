using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class ResourceSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CreditPoints",
                table: "ResourceUsageRecords",
                type: "numeric(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ResourceSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Pack = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PlanId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PayPalId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    State = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MonthlyPoints = table.Column<decimal>(type: "numeric(28,8)", precision: 28, scale: 8, nullable: false),
                    PriceExcludingTax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    VatPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceIncludingTax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextBillingAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastCreditedCycle = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceSubscriptions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResourceSubscriptionPayments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cycle = table.Column<int>(type: "integer", nullable: false),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GrantedPoints = table.Column<decimal>(type: "numeric(28,8)", precision: 28, scale: 8, nullable: false),
                    Reversed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceSubscriptionPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceSubscriptionPayments_ResourceSubscriptions_Subscrip~",
                        column: x => x.SubscriptionId,
                        principalTable: "ResourceSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceSubscriptionPayments_SubscriptionId_Cycle",
                table: "ResourceSubscriptionPayments",
                columns: new[] { "SubscriptionId", "Cycle" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceSubscriptions_PayPalId",
                table: "ResourceSubscriptions",
                column: "PayPalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceSubscriptions_UserId",
                table: "ResourceSubscriptions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResourceSubscriptionPayments");

            migrationBuilder.DropTable(
                name: "ResourceSubscriptions");

            migrationBuilder.DropColumn(
                name: "CreditPoints",
                table: "ResourceUsageRecords");
        }
    }
}
