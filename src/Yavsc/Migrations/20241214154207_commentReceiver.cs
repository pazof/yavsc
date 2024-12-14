using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class commentReceiver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogTrad");

            migrationBuilder.DropColumn(
                name: "Lang",
                table: "Blogspot");

            migrationBuilder.RenameColumn(
                name: "PostId",
                table: "Comment",
                newName: "ReceiverId"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Blogspot",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Photo",
                table: "Blogspot",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Blogspot",
                type: "character varying(56224)",
                maxLength: 56224,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReceiverId",
                table: "Comment",
                newName: "PostId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Blogspot",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024);

            migrationBuilder.AlterColumn<string>(
                name: "Photo",
                table: "Blogspot",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Blogspot",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(56224)",
                oldMaxLength: 56224);

            migrationBuilder.AddColumn<string>(
                name: "Lang",
                table: "Blogspot",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BlogTrad",
                columns: table => new
                {
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    Lang = table.Column<string>(type: "text", nullable: false),
                    TraducerId = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogTrad", x => new { x.PostId, x.Lang });
                    table.ForeignKey(
                        name: "FK_BlogTrad_AspNetUsers_TraducerId",
                        column: x => x.TraducerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogTrad_TraducerId",
                table: "BlogTrad",
                column: "TraducerId");
        }
    }
}
