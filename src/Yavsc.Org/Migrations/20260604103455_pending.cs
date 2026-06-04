using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class pending : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegexAlertPatterns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Pattern = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegexAlertPatterns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrustTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TokenSource = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TrustScore = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrustDeclarations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrustTokenId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeclarantTokenId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Sentiment = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ScoreDelta = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustDeclarations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrustDeclarations_TrustTokens_TrustTokenId",
                        column: x => x.TrustTokenId,
                        principalTable: "TrustTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeclarationFlag",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeclarationId = table.Column<long>(type: "bigint", nullable: false),
                    PatternId = table.Column<int>(type: "integer", nullable: false),
                    MatchExcerpt = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeclarationFlag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeclarationFlag_RegexAlertPatterns_PatternId",
                        column: x => x.PatternId,
                        principalTable: "RegexAlertPatterns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeclarationFlag_TrustDeclarations_DeclarationId",
                        column: x => x.DeclarationId,
                        principalTable: "TrustDeclarations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModerationLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeclarationId = table.Column<long>(type: "bigint", nullable: false),
                    ModeratorId = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    ScoreDelta = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationLogs", x => x.Id);
                    table.CheckConstraint("CK_ModerationLog_Immutable", "1=1");
                    table.ForeignKey(
                        name: "FK_ModerationLogs_TrustDeclarations_DeclarationId",
                        column: x => x.DeclarationId,
                        principalTable: "TrustDeclarations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeclarationFlag_DeclarationId",
                table: "DeclarationFlag",
                column: "DeclarationId");

            migrationBuilder.CreateIndex(
                name: "IX_DeclarationFlag_PatternId",
                table: "DeclarationFlag",
                column: "PatternId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationLogs_DeclarationId",
                table: "ModerationLogs",
                column: "DeclarationId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationLogs_ModeratorId",
                table: "ModerationLogs",
                column: "ModeratorId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationLogs_Timestamp",
                table: "ModerationLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_RegexAlertPatterns_IsActive",
                table: "RegexAlertPatterns",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TrustDeclarations_Status",
                table: "TrustDeclarations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TrustDeclarations_SubmittedAt",
                table: "TrustDeclarations",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TrustDeclarations_TrustTokenId",
                table: "TrustDeclarations",
                column: "TrustTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_TrustTokens_TokenHash",
                table: "TrustTokens",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeclarationFlag");

            migrationBuilder.DropTable(
                name: "ModerationLogs");

            migrationBuilder.DropTable(
                name: "RegexAlertPatterns");

            migrationBuilder.DropTable(
                name: "TrustDeclarations");

            migrationBuilder.DropTable(
                name: "TrustTokens");
        }
    }
}
