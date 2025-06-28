using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class blogPostPub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blogspotPublications",
                columns: table => new
                {
                    BlogpostId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blogspotPublications", x => x.BlogpostId);
                    table.ForeignKey(
                        name: "FK_blogspotPublications_BlogSpot_BlogpostId",
                        column: x => x.BlogpostId,
                        principalTable: "BlogSpot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blogspotPublications");
        }
    }
}
