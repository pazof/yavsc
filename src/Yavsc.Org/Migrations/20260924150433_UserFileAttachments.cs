using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class UserFileAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "UploadedFiles",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "UploadedFiles",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "UploadedFiles",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EstimateAttachedFiles",
                columns: table => new
                {
                    FileId = table.Column<long>(type: "bigint", nullable: false),
                    EstimateId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstimateAttachedFiles", x => new { x.FileId, x.EstimateId });
                    table.ForeignKey(
                        name: "FK_EstimateAttachedFiles_Estimates_EstimateId",
                        column: x => x.EstimateId,
                        principalTable: "Estimates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstimateAttachedFiles_UploadedFiles_FileId",
                        column: x => x.FileId,
                        principalTable: "UploadedFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QueryAttachedFiles",
                columns: table => new
                {
                    FileId = table.Column<long>(type: "bigint", nullable: false),
                    CommandId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryAttachedFiles", x => new { x.FileId, x.CommandId });
                    table.ForeignKey(
                        name: "FK_QueryAttachedFiles_NominativeServiceCommands_CommandId",
                        column: x => x.CommandId,
                        principalTable: "NominativeServiceCommands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QueryAttachedFiles_UploadedFiles_FileId",
                        column: x => x.FileId,
                        principalTable: "UploadedFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_OwnerId",
                table: "UploadedFiles",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_EstimateAttachedFiles_EstimateId",
                table: "EstimateAttachedFiles",
                column: "EstimateId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryAttachedFiles_CommandId",
                table: "QueryAttachedFiles",
                column: "CommandId");

            migrationBuilder.AddForeignKey(
                name: "FK_UploadedFiles_AspNetUsers_OwnerId",
                table: "UploadedFiles",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UploadedFiles_AspNetUsers_OwnerId",
                table: "UploadedFiles");

            migrationBuilder.DropTable(
                name: "EstimateAttachedFiles");

            migrationBuilder.DropTable(
                name: "QueryAttachedFiles");

            migrationBuilder.DropIndex(
                name: "IX_UploadedFiles_OwnerId",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "UploadedFiles");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "UploadedFiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "UploadedFiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
