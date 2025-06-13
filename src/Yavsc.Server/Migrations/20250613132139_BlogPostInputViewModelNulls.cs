using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class BlogPostInputViewModelNulls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "BlogSpot",
                type: "character varying(56224)",
                maxLength: 56224,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(56224)",
                oldMaxLength: 56224);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "BlogSpot",
                type: "character varying(56224)",
                maxLength: 56224,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(56224)",
                oldMaxLength: 56224,
                oldNullable: true);
        }
    }
}
