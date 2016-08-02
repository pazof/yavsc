using Microsoft.Data.Entity.Migrations;

namespace Yavsc.Migrations
{
    public partial class bcontentornot : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "bcontent", table: "Blog", newName:"Content");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "Content", table: "Blog", newName:"bcontent");
        }
    }
}
