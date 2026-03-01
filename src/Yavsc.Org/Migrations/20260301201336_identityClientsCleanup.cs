using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class identityClientsCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientClaim");

            migrationBuilder.DropTable(
                name: "ClientCorsOrigins");

            migrationBuilder.DropTable(
                name: "ClientGrantType");

            migrationBuilder.DropTable(
                name: "ClientIdPRestriction");

            migrationBuilder.DropTable(
                name: "ClientPostLogoutRedirectUri");

            migrationBuilder.DropTable(
                name: "ClientProperty");

            migrationBuilder.DropTable(
                name: "ClientRedirectUri");

            migrationBuilder.DropTable(
                name: "ClientScope");

            migrationBuilder.DropTable(
                name: "ClientSecret");

            migrationBuilder.DropTable(
                name: "Clients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AbsoluteRefreshTokenLifetime = table.Column<int>(type: "integer", nullable: false),
                    AccessTokenLifetime = table.Column<int>(type: "integer", nullable: false),
                    AccessTokenType = table.Column<int>(type: "integer", nullable: false),
                    AllowAccessTokensViaBrowser = table.Column<bool>(type: "boolean", nullable: false),
                    AllowOfflineAccess = table.Column<bool>(type: "boolean", nullable: false),
                    AllowPlainTextPkce = table.Column<bool>(type: "boolean", nullable: false),
                    AllowRememberConsent = table.Column<bool>(type: "boolean", nullable: false),
                    AllowedIdentityTokenSigningAlgorithms = table.Column<string>(type: "text", nullable: true),
                    AlwaysIncludeUserClaimsInIdToken = table.Column<bool>(type: "boolean", nullable: false),
                    AlwaysSendClientClaims = table.Column<bool>(type: "boolean", nullable: false),
                    AuthorizationCodeLifetime = table.Column<int>(type: "integer", nullable: false),
                    BackChannelLogoutSessionRequired = table.Column<bool>(type: "boolean", nullable: false),
                    BackChannelLogoutUri = table.Column<string>(type: "text", nullable: true),
                    ClientClaimsPrefix = table.Column<string>(type: "text", nullable: true),
                    ClientId = table.Column<string>(type: "text", nullable: true),
                    ClientName = table.Column<string>(type: "text", nullable: true),
                    ClientUri = table.Column<string>(type: "text", nullable: true),
                    ConsentLifetime = table.Column<int>(type: "integer", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DeviceCodeLifetime = table.Column<int>(type: "integer", nullable: false),
                    EnableLocalLogin = table.Column<bool>(type: "boolean", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    FrontChannelLogoutSessionRequired = table.Column<bool>(type: "boolean", nullable: false),
                    FrontChannelLogoutUri = table.Column<string>(type: "text", nullable: true),
                    IdentityTokenLifetime = table.Column<int>(type: "integer", nullable: false),
                    IncludeJwtId = table.Column<bool>(type: "boolean", nullable: false),
                    LastAccessed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LogoUri = table.Column<string>(type: "text", nullable: true),
                    NonEditable = table.Column<bool>(type: "boolean", nullable: false),
                    PairWiseSubjectSalt = table.Column<string>(type: "text", nullable: true),
                    ProtocolType = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiration = table.Column<int>(type: "integer", nullable: false),
                    RefreshTokenUsage = table.Column<int>(type: "integer", nullable: false),
                    RequireClientSecret = table.Column<bool>(type: "boolean", nullable: false),
                    RequireConsent = table.Column<bool>(type: "boolean", nullable: false),
                    RequirePkce = table.Column<bool>(type: "boolean", nullable: false),
                    RequireRequestObject = table.Column<bool>(type: "boolean", nullable: false),
                    SlidingRefreshTokenLifetime = table.Column<int>(type: "integer", nullable: false),
                    UpdateAccessTokenClaimsOnRefresh = table.Column<bool>(type: "boolean", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserCodeType = table.Column<string>(type: "text", nullable: true),
                    UserSsoLifetime = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientClaim",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientClaim_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientCorsOrigins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    Origin = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientCorsOrigins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientCorsOrigins_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientGrantType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    GrantType = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientGrantType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientGrantType_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientIdPRestriction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientIdPRestriction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientIdPRestriction_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientPostLogoutRedirectUri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    PostLogoutRedirectUri = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientPostLogoutRedirectUri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientPostLogoutRedirectUri_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientProperty",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProperty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientProperty_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientRedirectUri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    RedirectUri = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientRedirectUri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientRedirectUri_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientScope",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    Scope = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientScope", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientScope_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientSecret",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientSecret", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientSecret_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientClaim_ClientId",
                table: "ClientClaim",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientCorsOrigins_ClientId",
                table: "ClientCorsOrigins",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientGrantType_ClientId",
                table: "ClientGrantType",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientIdPRestriction_ClientId",
                table: "ClientIdPRestriction",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientPostLogoutRedirectUri_ClientId",
                table: "ClientPostLogoutRedirectUri",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientProperty_ClientId",
                table: "ClientProperty",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRedirectUri_ClientId",
                table: "ClientRedirectUri",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientScope_ClientId",
                table: "ClientScope",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientSecret_ClientId",
                table: "ClientSecret",
                column: "ClientId");
        }
    }
}
