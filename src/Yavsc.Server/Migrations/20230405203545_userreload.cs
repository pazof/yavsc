using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class userreload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Activities_ParentCode",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_BankIdentity_BankInfoId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Locations_PostalAddressId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_BankInfoId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BankInfoId",
                table: "AspNetUsers");

            migrationBuilder.Sql("delete from \"BankIdentity\"");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "BankIdentity",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "PostalAddressId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DedicatedGoogleCalendar",
                table: "AspNetUsers",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "ParentCode",
                table: "Activities",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_BankIdentity_UserId",
                table: "BankIdentity",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Activities_ParentCode",
                table: "Activities",
                column: "ParentCode",
                principalTable: "Activities",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Locations_PostalAddressId",
                table: "AspNetUsers",
                column: "PostalAddressId",
                principalTable: "Locations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BankIdentity_AspNetUsers_UserId",
                table: "BankIdentity",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Activities_ParentCode",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Locations_PostalAddressId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_BankIdentity_AspNetUsers_UserId",
                table: "BankIdentity");

            migrationBuilder.DropIndex(
                name: "IX_BankIdentity_UserId",
                table: "BankIdentity");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_AspNetUsers_Email",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "BankIdentity");

            migrationBuilder.AlterColumn<long>(
                name: "PostalAddressId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "DedicatedGoogleCalendar",
                table: "AspNetUsers",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "BankInfoId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "ParentCode",
                table: "Activities",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BankInfoId",
                table: "AspNetUsers",
                column: "BankInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Activities_ParentCode",
                table: "Activities",
                column: "ParentCode",
                principalTable: "Activities",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_BankIdentity_BankInfoId",
                table: "AspNetUsers",
                column: "BankInfoId",
                principalTable: "BankIdentity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Locations_PostalAddressId",
                table: "AspNetUsers",
                column: "PostalAddressId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
