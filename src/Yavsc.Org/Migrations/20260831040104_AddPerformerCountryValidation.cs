using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformerCountryValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExerciseCountryCode",
                table: "Performers",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "fr");

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "PerformerCodeInputValidations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    RegularExpression = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformerCodeInputValidations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformerCodeInputValidations_Countries_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "Countries",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Code", "DisplayName" },
                values: new object[,]
                {
                    { "en", "England" },
                    { "fr", "France" },
                    { "pt", "Portugal" }
                });

            migrationBuilder.InsertData(
                table: "PerformerCodeInputValidations",
                columns: new[] { "Id", "CountryCode", "ErrorMessage", "RegularExpression" },
                values: new object[,]
                {
                    { 1L, "fr", "Le code FR doit contenir entre 9 et 14 chiffres.", "^[0-9]{9,14}$" },
                    { 2L, "en", "Le code EN doit contenir entre 8 et 14 caracteres alphanumeriques.", "^[A-Za-z0-9]{8,14}$" },
                    { 3L, "pt", "Le code PT doit contenir exactement 9 chiffres.", "^[0-9]{9}$" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PerformerCodeInputValidations_CountryCode",
                table: "PerformerCodeInputValidations",
                column: "CountryCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerformerCodeInputValidations");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropColumn(
                name: "ExerciseCountryCode",
                table: "Performers");
        }
    }
}
