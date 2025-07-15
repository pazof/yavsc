using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class nullPaimentIdOnQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQueries_PayPalPayment_PaymentId",
                table: "HairCutQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_HairMultiCutQueries_PayPalPayment_PaymentId",
                table: "HairMultiCutQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_PayPalPayment_PaymentId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_RdvQueries_PayPalPayment_PaymentId",
                table: "RdvQueries");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "RdvQueries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "Project",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "HairMultiCutQueries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "HairCutQueries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQueries_PayPalPayment_PaymentId",
                table: "HairCutQueries",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken");

            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQueries_PayPalPayment_PaymentId",
                table: "HairMultiCutQueries",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_PayPalPayment_PaymentId",
                table: "Project",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken");

            migrationBuilder.AddForeignKey(
                name: "FK_RdvQueries_PayPalPayment_PaymentId",
                table: "RdvQueries",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQueries_PayPalPayment_PaymentId",
                table: "HairCutQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_HairMultiCutQueries_PayPalPayment_PaymentId",
                table: "HairMultiCutQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_PayPalPayment_PaymentId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_RdvQueries_PayPalPayment_PaymentId",
                table: "RdvQueries");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "RdvQueries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "Project",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "HairMultiCutQueries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "HairCutQueries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQueries_PayPalPayment_PaymentId",
                table: "HairCutQueries",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQueries_PayPalPayment_PaymentId",
                table: "HairMultiCutQueries",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_PayPalPayment_PaymentId",
                table: "Project",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RdvQueries_PayPalPayment_PaymentId",
                table: "RdvQueries",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
