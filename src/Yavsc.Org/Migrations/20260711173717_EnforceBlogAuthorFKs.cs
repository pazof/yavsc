using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class EnforceBlogAuthorFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Assainir les orphelins AVANT d'enforcer la FK Restrict.
            // En prod (Postgres), la migration aurait sinon planté
            // sur des billets/commentaires dont l'AuthorId pointe
            // vers un user déjà supprimé. La logique métier refuse
            // désormais l'orphelin (cf. BlogSpotService.Details) — on
            // aligne l'état de la base avec ce contrat.
            migrationBuilder.Sql(@"
                DO $$
                DECLARE n_comments int;
                        n_posts    int;
                BEGIN
                    DELETE FROM ""Comment""
                    WHERE ""AuthorId"" NOT IN (SELECT ""Id"" FROM ""AspNetUsers"");
                    GET DIAGNOSTICS n_comments = ROW_COUNT;

                    DELETE FROM ""BlogSpot""
                    WHERE ""AuthorId"" NOT IN (SELECT ""Id"" FROM ""AspNetUsers"");
                    GET DIAGNOSTICS n_posts = ROW_COUNT;

                    RAISE NOTICE 'EnforceBlogAuthorFKs: % orphaned comments deleted, % orphaned blog posts deleted',
                        n_comments, n_posts;
                END $$;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogSpot_AspNetUsers_AuthorId",
                table: "BlogSpot");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_AspNetUsers_AuthorId",
                table: "Comment");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogSpot_AspNetUsers_AuthorId",
                table: "BlogSpot",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_AspNetUsers_AuthorId",
                table: "Comment",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogSpot_AspNetUsers_AuthorId",
                table: "BlogSpot");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_AspNetUsers_AuthorId",
                table: "Comment");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogSpot_AspNetUsers_AuthorId",
                table: "BlogSpot",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_AspNetUsers_AuthorId",
                table: "Comment",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
