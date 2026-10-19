using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class SignedEstimateInvoicing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvoiceClientId",
                table: "NominativeServiceCommands",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceCurrency",
                table: "NominativeServiceCommands",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "InvoiceEstimateId",
                table: "NominativeServiceCommands",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceLinesJson",
                table: "NominativeServiceCommands",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoicePerformerId",
                table: "NominativeServiceCommands",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "InvoicedAtUtc",
                table: "NominativeServiceCommands",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentRevision",
                table: "NominativeServiceCommands",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "InvoicePayments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommandId = table.Column<long>(type: "bigint", nullable: false),
                    Source = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ReferenceScope = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Reference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoicePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoicePayments_NominativeServiceCommands_CommandId",
                        column: x => x.CommandId,
                        principalTable: "NominativeServiceCommands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NominativeServiceCommands_InvoiceEstimateId",
                table: "NominativeServiceCommands",
                column: "InvoiceEstimateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_CommandId",
                table: "InvoicePayments",
                column: "CommandId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_Source_ReferenceScope_Reference",
                table: "InvoicePayments",
                columns: new[] { "Source", "ReferenceScope", "Reference" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommands_Estimates_InvoiceEstimateId",
                table: "NominativeServiceCommands",
                column: "InvoiceEstimateId",
                principalTable: "Estimates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommands_Estimates_InvoiceEstimateId",
                table: "NominativeServiceCommands");

            migrationBuilder.DropTable(
                name: "InvoicePayments");

            migrationBuilder.DropIndex(
                name: "IX_NominativeServiceCommands_InvoiceEstimateId",
                table: "NominativeServiceCommands");

            migrationBuilder.DropColumn(
                name: "InvoiceClientId",
                table: "NominativeServiceCommands");

            migrationBuilder.DropColumn(
                name: "InvoiceCurrency",
                table: "NominativeServiceCommands");

            migrationBuilder.DropColumn(
                name: "InvoiceEstimateId",
                table: "NominativeServiceCommands");

            migrationBuilder.DropColumn(
                name: "InvoiceLinesJson",
                table: "NominativeServiceCommands");

            migrationBuilder.DropColumn(
                name: "InvoicePerformerId",
                table: "NominativeServiceCommands");

            migrationBuilder.DropColumn(
                name: "InvoicedAtUtc",
                table: "NominativeServiceCommands");

            migrationBuilder.DropColumn(
                name: "PaymentRevision",
                table: "NominativeServiceCommands");
        }
    }
}
