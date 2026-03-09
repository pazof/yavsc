using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ParentCode = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Photo = table.Column<string>(type: "text", nullable: true),
                    ModeratorGroupName = table.Column<string>(type: "text", nullable: true),
                    Rate = table.Column<int>(type: "integer", nullable: false),
                    SettingsClassName = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: true),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserModified = table.Column<string>(type: "text", nullable: true),
                    Hidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Code);
                    table.ForeignKey(
                        name: "FK_Activities_Activities_ParentCode",
                        column: x => x.ParentCode,
                        principalTable: "Activities",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "ApiResources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AllowedAccessTokenSigningAlgorithms = table.Column<string>(type: "text", nullable: true),
                    ShowInDiscoveryDocument = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAccessed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NonEditable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiResources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiScopes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    Emphasize = table.Column<bool>(type: "boolean", nullable: false),
                    ShowInDiscoveryDocument = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiScopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientProviderInfo",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    EMail = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    BillingAddressId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProviderInfo", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    ClientId = table.Column<string>(type: "text", nullable: true),
                    ProtocolType = table.Column<string>(type: "text", nullable: true),
                    RequireClientSecret = table.Column<bool>(type: "boolean", nullable: false),
                    ClientName = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ClientUri = table.Column<string>(type: "text", nullable: true),
                    LogoUri = table.Column<string>(type: "text", nullable: true),
                    RequireConsent = table.Column<bool>(type: "boolean", nullable: false),
                    AllowRememberConsent = table.Column<bool>(type: "boolean", nullable: false),
                    AlwaysIncludeUserClaimsInIdToken = table.Column<bool>(type: "boolean", nullable: false),
                    RequirePkce = table.Column<bool>(type: "boolean", nullable: false),
                    AllowPlainTextPkce = table.Column<bool>(type: "boolean", nullable: false),
                    RequireRequestObject = table.Column<bool>(type: "boolean", nullable: false),
                    AllowAccessTokensViaBrowser = table.Column<bool>(type: "boolean", nullable: false),
                    FrontChannelLogoutUri = table.Column<string>(type: "text", nullable: true),
                    FrontChannelLogoutSessionRequired = table.Column<bool>(type: "boolean", nullable: false),
                    BackChannelLogoutUri = table.Column<string>(type: "text", nullable: true),
                    BackChannelLogoutSessionRequired = table.Column<bool>(type: "boolean", nullable: false),
                    AllowOfflineAccess = table.Column<bool>(type: "boolean", nullable: false),
                    IdentityTokenLifetime = table.Column<int>(type: "integer", nullable: false),
                    AllowedIdentityTokenSigningAlgorithms = table.Column<string>(type: "text", nullable: true),
                    AccessTokenLifetime = table.Column<int>(type: "integer", nullable: false),
                    AuthorizationCodeLifetime = table.Column<int>(type: "integer", nullable: false),
                    ConsentLifetime = table.Column<int>(type: "integer", nullable: true),
                    AbsoluteRefreshTokenLifetime = table.Column<int>(type: "integer", nullable: false),
                    SlidingRefreshTokenLifetime = table.Column<int>(type: "integer", nullable: false),
                    RefreshTokenUsage = table.Column<int>(type: "integer", nullable: false),
                    UpdateAccessTokenClaimsOnRefresh = table.Column<bool>(type: "boolean", nullable: false),
                    RefreshTokenExpiration = table.Column<int>(type: "integer", nullable: false),
                    AccessTokenType = table.Column<int>(type: "integer", nullable: false),
                    EnableLocalLogin = table.Column<bool>(type: "boolean", nullable: false),
                    IncludeJwtId = table.Column<bool>(type: "boolean", nullable: false),
                    AlwaysSendClientClaims = table.Column<bool>(type: "boolean", nullable: false),
                    ClientClaimsPrefix = table.Column<string>(type: "text", nullable: true),
                    PairWiseSubjectSalt = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAccessed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserSsoLifetime = table.Column<int>(type: "integer", nullable: true),
                    UserCodeType = table.Column<string>(type: "text", nullable: true),
                    DeviceCodeLifetime = table.Column<int>(type: "integer", nullable: false),
                    NonEditable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Color",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Red = table.Column<byte>(type: "smallint", nullable: false),
                    Green = table.Column<byte>(type: "smallint", nullable: false),
                    Blue = table.Column<byte>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Color", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceFlowCodes",
                columns: table => new
                {
                    DeviceCode = table.Column<string>(type: "text", nullable: false),
                    UserCode = table.Column<string>(type: "text", nullable: false),
                    SubjectId = table.Column<string>(type: "text", nullable: true),
                    SessionId = table.Column<string>(type: "text", nullable: true),
                    ClientId = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Data = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceFlowCodes", x => new { x.UserCode, x.DeviceCode });
                });

            migrationBuilder.CreateTable(
                name: "DjSettings",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    SoundCloudId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DjSettings", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "EstimateTemplates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    OwnerId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstimateTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExceptionsSIREN",
                columns: table => new
                {
                    SIREN = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionsSIREN", x => x.SIREN);
                });

            migrationBuilder.CreateTable(
                name: "Feature",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShortName = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feature", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Form",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Form", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormationSettings",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormationSettings", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "GeneralSettings",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralSettings", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "HairPrestation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Length = table.Column<int>(type: "integer", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    Cut = table.Column<bool>(type: "boolean", nullable: false),
                    Dressing = table.Column<int>(type: "integer", nullable: false),
                    Tech = table.Column<int>(type: "integer", nullable: false),
                    Shampoo = table.Column<bool>(type: "boolean", nullable: false),
                    Cares = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HairPrestation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdentityResources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    Emphasize = table.Column<bool>(type: "boolean", nullable: false),
                    ShowInDiscoveryDocument = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NonEditable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityResources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Instrument",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrument", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Address = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MailingTemplate",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Topic = table.Column<string>(type: "text", nullable: true),
                    Body = table.Column<string>(type: "character varying(65536)", maxLength: 65536, nullable: true),
                    ReplyToAddress = table.Column<string>(type: "text", nullable: true),
                    ToSend = table.Column<int>(type: "integer", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: true),
                    UserModified = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailingTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MusicalTendency",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicalTendency", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    body = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    icon = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true, defaultValue: "exclam"),
                    sound = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    tag = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    color = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    click_action = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Target = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Option",
                columns: table => new
                {
                    CodeScrutin = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: true),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserModified = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Option", x => new { x.Code, x.CodeScrutin });
                });

            migrationBuilder.CreateTable(
                name: "Period",
                columns: table => new
                {
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Period", x => new { x.Start, x.End });
                });

            migrationBuilder.CreateTable(
                name: "PersistedGrants",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: true),
                    SubjectId = table.Column<string>(type: "text", nullable: true),
                    SessionId = table.Column<string>(type: "text", nullable: true),
                    ClientId = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConsumedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Data = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersistedGrants", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "PostalAddress",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Street1 = table.Column<string>(type: "text", nullable: true),
                    Street2 = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    Province = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalAddress", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Weight = table.Column<decimal>(type: "numeric", nullable: false),
                    Height = table.Column<decimal>(type: "numeric", nullable: false),
                    Width = table.Column<decimal>(type: "numeric", nullable: false),
                    Depth = table.Column<decimal>(type: "numeric", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Public = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteSkills",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Rate = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSkills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommandForm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActionName = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    ActivityCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandForm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommandForm_Activities_ActivityCode",
                        column: x => x.ActivityCode,
                        principalTable: "Activities",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContextId = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Public = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_Activities_ContextId",
                        column: x => x.ContextId,
                        principalTable: "Activities",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "ApiResourceClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApiResourceId = table.Column<int>(type: "integer", nullable: false),
                    ApiResourceId1 = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiResourceClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiResourceClaims_ApiResources_ApiResourceId",
                        column: x => x.ApiResourceId,
                        principalTable: "ApiResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApiResourceClaims_ApiResources_ApiResourceId1",
                        column: x => x.ApiResourceId1,
                        principalTable: "ApiResources",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApiResourceProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApiResourceId = table.Column<int>(type: "integer", nullable: false),
                    ApiResourceId1 = table.Column<int>(type: "integer", nullable: true),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiResourceProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiResourceProperties_ApiResources_ApiResourceId",
                        column: x => x.ApiResourceId,
                        principalTable: "ApiResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApiResourceProperties_ApiResources_ApiResourceId1",
                        column: x => x.ApiResourceId1,
                        principalTable: "ApiResources",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApiResourceScopes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Scope = table.Column<string>(type: "text", nullable: true),
                    ApiResourceId = table.Column<int>(type: "integer", nullable: false),
                    ApiResourceId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiResourceScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiResourceScopes_ApiResources_ApiResourceId",
                        column: x => x.ApiResourceId,
                        principalTable: "ApiResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApiResourceScopes_ApiResources_ApiResourceId1",
                        column: x => x.ApiResourceId1,
                        principalTable: "ApiResources",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApiResourceSecrets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApiResourceId = table.Column<int>(type: "integer", nullable: false),
                    ApiResourceId1 = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true),
                    Expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiResourceSecrets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiResourceSecrets_ApiResources_ApiResourceId",
                        column: x => x.ApiResourceId,
                        principalTable: "ApiResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApiResourceSecrets_ApiResources_ApiResourceId1",
                        column: x => x.ApiResourceId1,
                        principalTable: "ApiResources",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApiScopeClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScopeId = table.Column<int>(type: "integer", nullable: false),
                    ScopeId1 = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiScopeClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiScopeClaims_ApiScopes_ScopeId",
                        column: x => x.ScopeId,
                        principalTable: "ApiScopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApiScopeClaims_ApiScopes_ScopeId1",
                        column: x => x.ScopeId1,
                        principalTable: "ApiScopes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApiScopeProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScopeId = table.Column<int>(type: "integer", nullable: false),
                    ScopeId1 = table.Column<int>(type: "integer", nullable: true),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiScopeProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiScopeProperties_ApiScopes_ScopeId",
                        column: x => x.ScopeId,
                        principalTable: "ApiScopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApiScopeProperties_ApiScopes_ScopeId1",
                        column: x => x.ScopeId1,
                        principalTable: "ApiScopes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true),
                    ClientId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientClaims_Clients_ClientId",
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
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Origin = table.Column<string>(type: "text", nullable: true),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    ClientId1 = table.Column<int>(type: "integer", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_ClientCorsOrigins_Clients_ClientId1",
                        column: x => x.ClientId1,
                        principalTable: "Clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClientGrantTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    GrantType = table.Column<string>(type: "text", nullable: true),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    ClientId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientGrantTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientGrantTypes_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientGrantTypes_Clients_ClientId1",
                        column: x => x.ClientId1,
                        principalTable: "Clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClientIdPRestrictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Provider = table.Column<string>(type: "text", nullable: true),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    ClientId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientIdPRestrictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientIdPRestrictions_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientIdPRestrictions_Clients_ClientId1",
                        column: x => x.ClientId1,
                        principalTable: "Clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClientPostLogoutRedirectUris",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    PostLogoutRedirectUri = table.Column<string>(type: "text", nullable: true),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    ClientId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientPostLogoutRedirectUris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientPostLogoutRedirectUris_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientPostLogoutRedirectUris_Clients_ClientId1",
                        column: x => x.ClientId1,
                        principalTable: "Clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClientProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    ClientId1 = table.Column<int>(type: "integer", nullable: true),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientProperties_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientProperties_Clients_ClientId1",
                        column: x => x.ClientId1,
                        principalTable: "Clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClientRedirectUris",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    RedirectUri = table.Column<string>(type: "text", nullable: true),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    ClientId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientRedirectUris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientRedirectUris_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientRedirectUris_Clients_ClientId1",
                        column: x => x.ClientId1,
                        principalTable: "Clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClientScopes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Scope = table.Column<string>(type: "text", nullable: true),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    ClientId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientScopes_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientScopes_Clients_ClientId1",
                        column: x => x.ClientId1,
                        principalTable: "Clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClientSecrets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    ClientId1 = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true),
                    Expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientSecrets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientSecrets_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientSecrets_Clients_ClientId1",
                        column: x => x.ClientId1,
                        principalTable: "Clients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HairTaint",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Brand = table.Column<string>(type: "text", nullable: true),
                    ColorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HairTaint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HairTaint_Color_ColorId",
                        column: x => x.ColorId,
                        principalTable: "Color",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bug",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FeatureId = table.Column<long>(type: "bigint", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bug", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bug_Feature_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Feature",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IdentityResourceClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdentityResourceId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityResourceClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityResourceClaims_IdentityResources_IdentityResourceId",
                        column: x => x.IdentityResourceId,
                        principalTable: "IdentityResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IdentityResourceProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdentityResourceId = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityResourceProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityResourceProperties_IdentityResources_IdentityResour~",
                        column: x => x.IdentityResourceId,
                        principalTable: "IdentityResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Avatar = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true, defaultValue: "/images/Users/icon_user.png"),
                    FullName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    PostalAddressId = table.Column<long>(type: "bigint", nullable: true),
                    DedicatedGoogleCalendar = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DiskQuota = table.Column<long>(type: "bigint", nullable: false, defaultValue: 524288000L),
                    DiskUsage = table.Column<long>(type: "bigint", nullable: false),
                    MaxFileSize = table.Column<long>(type: "bigint", nullable: false),
                    AllowMonthlyEmail = table.Column<bool>(type: "boolean", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.UniqueConstraint("AK_AspNetUsers_Email", x => x.Email);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Locations_PostalAddressId",
                        column: x => x.PostalAddressId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MusicalPreference",
                columns: table => new
                {
                    OwnerProfileId = table.Column<string>(type: "text", nullable: false),
                    Rate = table.Column<int>(type: "integer", nullable: false),
                    TendencyId = table.Column<long>(type: "bigint", nullable: false),
                    DjSettingsUserId = table.Column<string>(type: "text", nullable: true),
                    GeneralSettingsUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicalPreference", x => x.OwnerProfileId);
                    table.ForeignKey(
                        name: "FK_MusicalPreference_DjSettings_DjSettingsUserId",
                        column: x => x.DjSettingsUserId,
                        principalTable: "DjSettings",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_MusicalPreference_GeneralSettings_GeneralSettingsUserId",
                        column: x => x.GeneralSettingsUserId,
                        principalTable: "GeneralSettings",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_MusicalPreference_MusicalTendency_TendencyId",
                        column: x => x.TendencyId,
                        principalTable: "MusicalTendency",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HairTaintInstance",
                columns: table => new
                {
                    TaintId = table.Column<long>(type: "bigint", nullable: false),
                    PrestationId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HairTaintInstance", x => new { x.TaintId, x.PrestationId });
                    table.ForeignKey(
                        name: "FK_HairTaintInstance_HairPrestation_PrestationId",
                        column: x => x.PrestationId,
                        principalTable: "HairPrestation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairTaintInstance_HairTaint_TaintId",
                        column: x => x.TaintId,
                        principalTable: "HairTaint",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Announce",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    For = table.Column<byte>(type: "smallint", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Topic = table.Column<string>(type: "text", nullable: true),
                    Sender = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announce", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Announce_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ban",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: true),
                    UserModified = table.Column<string>(type: "text", nullable: true),
                    TargetId = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ban", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ban_AspNetUsers_TargetId",
                        column: x => x.TargetId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankIdentity",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BIC = table.Column<string>(type: "text", nullable: true),
                    IBAN = table.Column<string>(type: "text", nullable: true),
                    BankCode = table.Column<string>(type: "text", nullable: true),
                    WicketCode = table.Column<string>(type: "text", nullable: true),
                    AccountNumber = table.Column<string>(type: "text", nullable: true),
                    BankedKey = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankIdentity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankIdentity_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BankStatus",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Credits = table.Column<decimal>(type: "numeric", nullable: false),
                    ContactCredits = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatus", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_BankStatus_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlackListed",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlackListed", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlackListed_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlackListed_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogSpot",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Photo = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Title = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Article = table.Column<string>(type: "character varying(56224)", maxLength: 56224, nullable: true),
                    AuthorId = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: true),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserModified = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogSpot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogSpot_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChatConnection",
                columns: table => new
                {
                    ConnectionId = table.Column<string>(type: "text", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    Connected = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatConnection", x => x.ConnectionId);
                    table.ForeignKey(
                        name: "FK_ChatConnection_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatRoom",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Topic = table.Column<string>(type: "text", nullable: true),
                    OwnerId = table.Column<string>(type: "text", nullable: true),
                    LatestJoinPart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: true),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserModified = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRoom", x => x.Name);
                    table.ForeignKey(
                        name: "FK_ChatRoom_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Circle",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Public = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Circle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Circle_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    EMail = table.Column<string>(type: "text", nullable: true),
                    AddressId = table.Column<long>(type: "bigint", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => new { x.OwnerId, x.UserId });
                    table.ForeignKey(
                        name: "FK_Contact_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contact_PostalAddress_AddressId",
                        column: x => x.AddressId,
                        principalTable: "PostalAddress",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceDeclaration",
                columns: table => new
                {
                    DeviceId = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: true),
                    Platform = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<string>(type: "text", nullable: true),
                    DeviceOwnerId = table.Column<string>(type: "text", nullable: true),
                    DeclarationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "LOCALTIMESTAMP"),
                    LatestActivityUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceDeclaration", x => x.DeviceId);
                    table.ForeignKey(
                        name: "FK_DeviceDeclaration_AspNetUsers_DeviceOwnerId",
                        column: x => x.DeviceOwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DismissClicked",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    NotificationId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DismissClicked", x => new { x.UserId, x.NotificationId });
                    table.ForeignKey(
                        name: "FK_DismissClicked_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DismissClicked_Notification_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GitRepositoryReference",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Path = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: true),
                    Branch = table.Column<string>(type: "text", nullable: true),
                    OwnerId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitRepositoryReference", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GitRepositoryReference_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LiveFlow",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Pitch = table.Column<string>(type: "text", nullable: true),
                    MediaType = table.Column<string>(type: "text", nullable: true),
                    DifferedFileName = table.Column<string>(type: "text", nullable: true),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveFlow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveFlow_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayPalPayment",
                columns: table => new
                {
                    CreationToken = table.Column<string>(type: "text", nullable: false),
                    ExecutorId = table.Column<string>(type: "text", nullable: false),
                    PaypalPayerId = table.Column<string>(type: "text", nullable: true),
                    OrderReference = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: true),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserModified = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayPalPayment", x => x.CreationToken);
                    table.ForeignKey(
                        name: "FK_PayPalPayment_AspNetUsers_ExecutorId",
                        column: x => x.ExecutorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Performers",
                columns: table => new
                {
                    PerformerId = table.Column<string>(type: "text", nullable: false),
                    SIREN = table.Column<string>(type: "text", nullable: false),
                    OrganizationAddressId = table.Column<long>(type: "bigint", nullable: false),
                    AcceptNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    AcceptPublicContact = table.Column<bool>(type: "boolean", nullable: false),
                    UseGeoLocalizationToReduceDistanceWithClients = table.Column<bool>(type: "boolean", nullable: false),
                    WebSite = table.Column<string>(type: "text", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    MaxDailyCost = table.Column<int>(type: "integer", nullable: true),
                    MinDailyCost = table.Column<int>(type: "integer", nullable: true),
                    Rate = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Performers", x => x.PerformerId);
                    table.ForeignKey(
                        name: "FK_Performers_AspNetUsers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Performers_Locations_OrganizationAddressId",
                        column: x => x.OrganizationAddressId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Schedule",
                columns: table => new
                {
                    OwnerId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule", x => x.OwnerId);
                    table.ForeignKey(
                        name: "FK_Schedule_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BalanceImpact",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Impact = table.Column<decimal>(type: "numeric", nullable: false),
                    ExecDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    BalanceId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceImpact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BalanceImpact_BankStatus_BalanceId",
                        column: x => x.BalanceId,
                        principalTable: "BankStatus",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blogSpotPublications",
                columns: table => new
                {
                    BlogpostId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blogSpotPublications", x => x.BlogpostId);
                    table.ForeignKey(
                        name: "FK_blogSpotPublications_BlogSpot_BlogpostId",
                        column: x => x.BlogpostId,
                        principalTable: "BlogSpot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogTag",
                columns: table => new
                {
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    TagId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogTag", x => new { x.PostId, x.TagId });
                    table.ForeignKey(
                        name: "FK_BlogTag_BlogSpot_PostId",
                        column: x => x.PostId,
                        principalTable: "BlogSpot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogTag_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Article = table.Column<string>(type: "text", nullable: true),
                    ReceiverId = table.Column<long>(type: "bigint", nullable: false),
                    Visible = table.Column<bool>(type: "boolean", nullable: false),
                    AuthorId = table.Column<string>(type: "text", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: true),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserModified = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comment_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comment_BlogSpot_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "BlogSpot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comment_Comment_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Comment",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChatRoomAccess",
                columns: table => new
                {
                    ChannelName = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRoomAccess", x => new { x.ChannelName, x.UserId });
                    table.ForeignKey(
                        name: "FK_ChatRoomAccess_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatRoomAccess_ChatRoom_ChannelName",
                        column: x => x.ChannelName,
                        principalTable: "ChatRoom",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CircleAuthorizationToBlogPost",
                columns: table => new
                {
                    CircleId = table.Column<long>(type: "bigint", nullable: false),
                    BlogPostId = table.Column<long>(type: "bigint", nullable: false),
                    Comment = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleAuthorizationToBlogPost", x => new { x.CircleId, x.BlogPostId });
                    table.ForeignKey(
                        name: "FK_CircleAuthorizationToBlogPost_BlogSpot_BlogPostId",
                        column: x => x.BlogPostId,
                        principalTable: "BlogSpot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CircleAuthorizationToBlogPost_Circle_CircleId",
                        column: x => x.CircleId,
                        principalTable: "Circle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CircleMembers",
                columns: table => new
                {
                    CircleId = table.Column<long>(type: "bigint", nullable: false),
                    MemberId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleMembers", x => new { x.MemberId, x.CircleId });
                    table.ForeignKey(
                        name: "FK_CircleMembers_AspNetUsers_MemberId",
                        column: x => x.MemberId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CircleMembers_Circle_CircleId",
                        column: x => x.CircleId,
                        principalTable: "Circle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoWorking",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerformerId = table.Column<string>(type: "text", nullable: true),
                    WorkingForId = table.Column<string>(type: "text", nullable: true),
                    FormationSettingsUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoWorking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoWorking_AspNetUsers_WorkingForId",
                        column: x => x.WorkingForId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CoWorking_FormationSettings_FormationSettingsUserId",
                        column: x => x.FormationSettingsUserId,
                        principalTable: "FormationSettings",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_CoWorking_Performers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "Performers",
                        principalColumn: "PerformerId");
                });

            migrationBuilder.CreateTable(
                name: "HairMultiCutQueries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LocationId = table.Column<long>(type: "bigint", nullable: true),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Consent = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: true),
                    UserModified = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ClientId = table.Column<string>(type: "text", nullable: false),
                    PerformerId = table.Column<string>(type: "text", nullable: false),
                    ValidationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Previsional = table.Column<decimal>(type: "numeric", nullable: true),
                    ActivityCode = table.Column<string>(type: "text", nullable: false),
                    Decided = table.Column<bool>(type: "boolean", nullable: false),
                    PaymentId = table.Column<string>(type: "text", nullable: true),
                    Accepted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HairMultiCutQueries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HairMultiCutQueries_Activities_ActivityCode",
                        column: x => x.ActivityCode,
                        principalTable: "Activities",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairMultiCutQueries_AspNetUsers_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairMultiCutQueries_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HairMultiCutQueries_PayPalPayment_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "PayPalPayment",
                        principalColumn: "CreationToken");
                    table.ForeignKey(
                        name: "FK_HairMultiCutQueries_Performers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "Performers",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Instrumentation",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    InstrumentId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrumentation", x => new { x.InstrumentId, x.UserId });
                    table.ForeignKey(
                        name: "FK_Instrumentation_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Instrumentation_Performers_UserId",
                        column: x => x.UserId,
                        principalTable: "Performers",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentRating",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InstrumentId = table.Column<long>(type: "bigint", nullable: false),
                    Rate = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentRating", x => x.Id);
                    table.UniqueConstraint("AK_InstrumentRating_InstrumentId_OwnerId", x => new { x.InstrumentId, x.OwnerId });
                    table.ForeignKey(
                        name: "FK_InstrumentRating_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstrumentRating_Performers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Performers",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerId = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true),
                    GitId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Consent = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: true),
                    UserModified = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ClientId = table.Column<string>(type: "text", nullable: false),
                    PerformerId = table.Column<string>(type: "text", nullable: false),
                    ValidationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Previsional = table.Column<decimal>(type: "numeric", nullable: true),
                    ActivityCode = table.Column<string>(type: "text", nullable: false),
                    Decided = table.Column<bool>(type: "boolean", nullable: false),
                    PaymentId = table.Column<string>(type: "text", nullable: true),
                    Accepted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_Activities_ActivityCode",
                        column: x => x.ActivityCode,
                        principalTable: "Activities",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Project_AspNetUsers_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Project_GitRepositoryReference_GitId",
                        column: x => x.GitId,
                        principalTable: "GitRepositoryReference",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Project_PayPalPayment_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "PayPalPayment",
                        principalColumn: "CreationToken");
                    table.ForeignKey(
                        name: "FK_Project_Performers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "Performers",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RdvQueries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationId = table.Column<long>(type: "bigint", nullable: true),
                    LocationType = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Consent = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: true),
                    UserModified = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ClientId = table.Column<string>(type: "text", nullable: false),
                    PerformerId = table.Column<string>(type: "text", nullable: false),
                    ValidationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Previsional = table.Column<decimal>(type: "numeric", nullable: true),
                    ActivityCode = table.Column<string>(type: "text", nullable: false),
                    Decided = table.Column<bool>(type: "boolean", nullable: false),
                    PaymentId = table.Column<string>(type: "text", nullable: true),
                    Accepted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RdvQueries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RdvQueries_Activities_ActivityCode",
                        column: x => x.ActivityCode,
                        principalTable: "Activities",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RdvQueries_AspNetUsers_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RdvQueries_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RdvQueries_PayPalPayment_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "PayPalPayment",
                        principalColumn: "CreationToken");
                    table.ForeignKey(
                        name: "FK_RdvQueries_Performers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "Performers",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserActivities",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    DoesCode = table.Column<string>(type: "text", nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivities", x => new { x.DoesCode, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserActivities_Activities_DoesCode",
                        column: x => x.DoesCode,
                        principalTable: "Activities",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserActivities_Performers_UserId",
                        column: x => x.UserId,
                        principalTable: "Performers",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BrusherProfile",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ActionDistance = table.Column<int>(type: "integer", nullable: false),
                    ScheduleOwnerId = table.Column<string>(type: "text", nullable: true),
                    WomenLongCutPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    WomenHalfCutPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    WomenShortCutPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ManCutPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ManBrushPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    KidCutPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    LongBrushingPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    HalfBrushingPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ShortBrushingPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    LongColorPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    HalfColorPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ShortColorPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    LongMultiColorPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    HalfMultiColorPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ShortMultiColorPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    LongPermanentPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    HalfPermanentPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ShortPermanentPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    LongDefrisPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    HalfDefrisPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ShortDefrisPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    LongMechPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    HalfMechPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ShortMechPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    LongBalayagePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    HalfBalayagePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ShortBalayagePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    LongFoldingPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    HalfFoldingPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ShortFoldingPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ShampooPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    CarePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    FlatFeeDiscount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrusherProfile", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_BrusherProfile_Performers_UserId",
                        column: x => x.UserId,
                        principalTable: "Performers",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BrusherProfile_Schedule_ScheduleOwnerId",
                        column: x => x.ScheduleOwnerId,
                        principalTable: "Schedule",
                        principalColumn: "OwnerId");
                });

            migrationBuilder.CreateTable(
                name: "ScheduledEvent",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reccurence = table.Column<int>(type: "integer", nullable: false),
                    ScheduleOwnerId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledEvent_Period_PeriodStart_PeriodEnd",
                        columns: x => new { x.PeriodStart, x.PeriodEnd },
                        principalTable: "Period",
                        principalColumns: new[] { "Start", "End" });
                    table.ForeignKey(
                        name: "FK_ScheduledEvent_Schedule_ScheduleOwnerId",
                        column: x => x.ScheduleOwnerId,
                        principalTable: "Schedule",
                        principalColumn: "OwnerId");
                });

            migrationBuilder.CreateTable(
                name: "HairPrestationCollectionItem",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrestationId = table.Column<long>(type: "bigint", nullable: false),
                    QueryId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HairPrestationCollectionItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HairPrestationCollectionItem_HairMultiCutQueries_QueryId",
                        column: x => x.QueryId,
                        principalTable: "HairMultiCutQueries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairPrestationCollectionItem_HairPrestation_PrestationId",
                        column: x => x.PrestationId,
                        principalTable: "HairPrestation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectBuildConfiguration",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectBuildConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectBuildConfiguration_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Estimates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommandId = table.Column<long>(type: "bigint", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    AttachedGraphicsString = table.Column<string>(type: "text", nullable: true),
                    AttachedFilesString = table.Column<string>(type: "text", nullable: true),
                    OwnerId = table.Column<string>(type: "text", nullable: true),
                    ClientId = table.Column<string>(type: "text", nullable: false),
                    CommandType = table.Column<string>(type: "text", nullable: false),
                    ProviderValidationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClientValidationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estimates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Estimates_AspNetUsers_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Estimates_Performers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Performers",
                        principalColumn: "PerformerId");
                    table.ForeignKey(
                        name: "FK_Estimates_RdvQueries_CommandId",
                        column: x => x.CommandId,
                        principalTable: "RdvQueries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HairCutQueries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: true),
                    PrestationId = table.Column<long>(type: "bigint", nullable: false),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LocationId = table.Column<long>(type: "bigint", nullable: true),
                    AdditionalInfo = table.Column<string>(type: "text", nullable: true),
                    SelectedProfileUserId = table.Column<string>(type: "text", nullable: true),
                    Consent = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: true),
                    UserModified = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ClientId = table.Column<string>(type: "text", nullable: false),
                    PerformerId = table.Column<string>(type: "text", nullable: false),
                    ValidationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Previsional = table.Column<decimal>(type: "numeric", nullable: true),
                    ActivityCode = table.Column<string>(type: "text", nullable: false),
                    Decided = table.Column<bool>(type: "boolean", nullable: false),
                    PaymentId = table.Column<string>(type: "text", nullable: true),
                    Accepted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HairCutQueries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HairCutQueries_Activities_ActivityCode",
                        column: x => x.ActivityCode,
                        principalTable: "Activities",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairCutQueries_AspNetUsers_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairCutQueries_BrusherProfile_SelectedProfileUserId",
                        column: x => x.SelectedProfileUserId,
                        principalTable: "BrusherProfile",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_HairCutQueries_HairPrestation_PrestationId",
                        column: x => x.PrestationId,
                        principalTable: "HairPrestation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairCutQueries_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HairCutQueries_PayPalPayment_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "PayPalPayment",
                        principalColumn: "CreationToken");
                    table.ForeignKey(
                        name: "FK_HairCutQueries_Performers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "Performers",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HyperLink",
                columns: table => new
                {
                    HRef = table.Column<string>(type: "text", nullable: false),
                    Method = table.Column<string>(type: "text", nullable: false),
                    Rel = table.Column<string>(type: "text", nullable: true),
                    ContentType = table.Column<string>(type: "text", nullable: true),
                    BrusherProfileUserId = table.Column<string>(type: "text", nullable: true),
                    PayPalPaymentCreationToken = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HyperLink", x => new { x.HRef, x.Method });
                    table.ForeignKey(
                        name: "FK_HyperLink_BrusherProfile_BrusherProfileUserId",
                        column: x => x.BrusherProfileUserId,
                        principalTable: "BrusherProfile",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_HyperLink_PayPalPayment_PayPalPaymentCreationToken",
                        column: x => x.PayPalPaymentCreationToken,
                        principalTable: "PayPalPayment",
                        principalColumn: "CreationToken");
                });

            migrationBuilder.CreateTable(
                name: "CommandLine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    UnitaryCost = table.Column<decimal>(type: "numeric", nullable: false),
                    EstimateId = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: true),
                    EstimateTemplateId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommandLine_EstimateTemplates_EstimateTemplateId",
                        column: x => x.EstimateTemplateId,
                        principalTable: "EstimateTemplates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CommandLine_Estimates_EstimateId",
                        column: x => x.EstimateId,
                        principalTable: "Estimates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ParentCode",
                table: "Activities",
                column: "ParentCode");

            migrationBuilder.CreateIndex(
                name: "IX_Announce_OwnerId",
                table: "Announce",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiResourceClaims_ApiResourceId",
                table: "ApiResourceClaims",
                column: "ApiResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiResourceClaims_ApiResourceId1",
                table: "ApiResourceClaims",
                column: "ApiResourceId1");

            migrationBuilder.CreateIndex(
                name: "IX_ApiResourceProperties_ApiResourceId",
                table: "ApiResourceProperties",
                column: "ApiResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiResourceProperties_ApiResourceId1",
                table: "ApiResourceProperties",
                column: "ApiResourceId1");

            migrationBuilder.CreateIndex(
                name: "IX_ApiResourceScopes_ApiResourceId",
                table: "ApiResourceScopes",
                column: "ApiResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiResourceScopes_ApiResourceId1",
                table: "ApiResourceScopes",
                column: "ApiResourceId1");

            migrationBuilder.CreateIndex(
                name: "IX_ApiResourceSecrets_ApiResourceId",
                table: "ApiResourceSecrets",
                column: "ApiResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiResourceSecrets_ApiResourceId1",
                table: "ApiResourceSecrets",
                column: "ApiResourceId1");

            migrationBuilder.CreateIndex(
                name: "IX_ApiScopeClaims_ScopeId",
                table: "ApiScopeClaims",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiScopeClaims_ScopeId1",
                table: "ApiScopeClaims",
                column: "ScopeId1");

            migrationBuilder.CreateIndex(
                name: "IX_ApiScopeProperties_ScopeId",
                table: "ApiScopeProperties",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiScopeProperties_ScopeId1",
                table: "ApiScopeProperties",
                column: "ScopeId1");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PostalAddressId",
                table: "AspNetUsers",
                column: "PostalAddressId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BalanceImpact_BalanceId",
                table: "BalanceImpact",
                column: "BalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Ban_TargetId",
                table: "Ban",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_BankIdentity_UserId",
                table: "BankIdentity",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlackListed_OwnerId",
                table: "BlackListed",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_BlackListed_UserId",
                table: "BlackListed",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogSpot_AuthorId",
                table: "BlogSpot",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogTag_TagId",
                table: "BlogTag",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_BrusherProfile_ScheduleOwnerId",
                table: "BrusherProfile",
                column: "ScheduleOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bug_FeatureId",
                table: "Bug",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatConnection_ApplicationUserId",
                table: "ChatConnection",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoom_OwnerId",
                table: "ChatRoom",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomAccess_UserId",
                table: "ChatRoomAccess",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Circle_ApplicationUserId",
                table: "Circle",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CircleAuthorizationToBlogPost_BlogPostId",
                table: "CircleAuthorizationToBlogPost",
                column: "BlogPostId");

            migrationBuilder.CreateIndex(
                name: "IX_CircleMembers_CircleId",
                table: "CircleMembers",
                column: "CircleId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientClaims_ClientId",
                table: "ClientClaims",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientCorsOrigins_ClientId",
                table: "ClientCorsOrigins",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientCorsOrigins_ClientId1",
                table: "ClientCorsOrigins",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_ClientGrantTypes_ClientId",
                table: "ClientGrantTypes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientGrantTypes_ClientId1",
                table: "ClientGrantTypes",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_ClientIdPRestrictions_ClientId",
                table: "ClientIdPRestrictions",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientIdPRestrictions_ClientId1",
                table: "ClientIdPRestrictions",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_ClientPostLogoutRedirectUris_ClientId",
                table: "ClientPostLogoutRedirectUris",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientPostLogoutRedirectUris_ClientId1",
                table: "ClientPostLogoutRedirectUris",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_ClientProperties_ClientId",
                table: "ClientProperties",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientProperties_ClientId1",
                table: "ClientProperties",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRedirectUris_ClientId",
                table: "ClientRedirectUris",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRedirectUris_ClientId1",
                table: "ClientRedirectUris",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_ClientScopes_ClientId",
                table: "ClientScopes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientScopes_ClientId1",
                table: "ClientScopes",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_ClientSecrets_ClientId",
                table: "ClientSecrets",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientSecrets_ClientId1",
                table: "ClientSecrets",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_CommandForm_ActivityCode",
                table: "CommandForm",
                column: "ActivityCode");

            migrationBuilder.CreateIndex(
                name: "IX_CommandLine_EstimateId",
                table: "CommandLine",
                column: "EstimateId");

            migrationBuilder.CreateIndex(
                name: "IX_CommandLine_EstimateTemplateId",
                table: "CommandLine",
                column: "EstimateTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_AuthorId",
                table: "Comment",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_ParentId",
                table: "Comment",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_ReceiverId",
                table: "Comment",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Contact_AddressId",
                table: "Contact",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Contact_ApplicationUserId",
                table: "Contact",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CoWorking_FormationSettingsUserId",
                table: "CoWorking",
                column: "FormationSettingsUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CoWorking_PerformerId",
                table: "CoWorking",
                column: "PerformerId");

            migrationBuilder.CreateIndex(
                name: "IX_CoWorking_WorkingForId",
                table: "CoWorking",
                column: "WorkingForId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceDeclaration_DeviceOwnerId",
                table: "DeviceDeclaration",
                column: "DeviceOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DismissClicked_NotificationId",
                table: "DismissClicked",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Estimates_ClientId",
                table: "Estimates",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Estimates_CommandId",
                table: "Estimates",
                column: "CommandId");

            migrationBuilder.CreateIndex(
                name: "IX_Estimates_OwnerId",
                table: "Estimates",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_GitRepositoryReference_OwnerId",
                table: "GitRepositoryReference",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_HairCutQueries_ActivityCode",
                table: "HairCutQueries",
                column: "ActivityCode");

            migrationBuilder.CreateIndex(
                name: "IX_HairCutQueries_ClientId",
                table: "HairCutQueries",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_HairCutQueries_LocationId",
                table: "HairCutQueries",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_HairCutQueries_PaymentId",
                table: "HairCutQueries",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_HairCutQueries_PerformerId",
                table: "HairCutQueries",
                column: "PerformerId");

            migrationBuilder.CreateIndex(
                name: "IX_HairCutQueries_PrestationId",
                table: "HairCutQueries",
                column: "PrestationId");

            migrationBuilder.CreateIndex(
                name: "IX_HairCutQueries_SelectedProfileUserId",
                table: "HairCutQueries",
                column: "SelectedProfileUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HairMultiCutQueries_ActivityCode",
                table: "HairMultiCutQueries",
                column: "ActivityCode");

            migrationBuilder.CreateIndex(
                name: "IX_HairMultiCutQueries_ClientId",
                table: "HairMultiCutQueries",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_HairMultiCutQueries_LocationId",
                table: "HairMultiCutQueries",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_HairMultiCutQueries_PaymentId",
                table: "HairMultiCutQueries",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_HairMultiCutQueries_PerformerId",
                table: "HairMultiCutQueries",
                column: "PerformerId");

            migrationBuilder.CreateIndex(
                name: "IX_HairPrestationCollectionItem_PrestationId",
                table: "HairPrestationCollectionItem",
                column: "PrestationId");

            migrationBuilder.CreateIndex(
                name: "IX_HairPrestationCollectionItem_QueryId",
                table: "HairPrestationCollectionItem",
                column: "QueryId");

            migrationBuilder.CreateIndex(
                name: "IX_HairTaint_ColorId",
                table: "HairTaint",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_HairTaintInstance_PrestationId",
                table: "HairTaintInstance",
                column: "PrestationId");

            migrationBuilder.CreateIndex(
                name: "IX_HyperLink_BrusherProfileUserId",
                table: "HyperLink",
                column: "BrusherProfileUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HyperLink_PayPalPaymentCreationToken",
                table: "HyperLink",
                column: "PayPalPaymentCreationToken");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityResourceClaims_IdentityResourceId",
                table: "IdentityResourceClaims",
                column: "IdentityResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityResourceProperties_IdentityResourceId",
                table: "IdentityResourceProperties",
                column: "IdentityResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Instrumentation_UserId",
                table: "Instrumentation",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentRating_OwnerId",
                table: "InstrumentRating",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveFlow_OwnerId",
                table: "LiveFlow",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicalPreference_DjSettingsUserId",
                table: "MusicalPreference",
                column: "DjSettingsUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicalPreference_GeneralSettingsUserId",
                table: "MusicalPreference",
                column: "GeneralSettingsUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicalPreference_TendencyId",
                table: "MusicalPreference",
                column: "TendencyId");

            migrationBuilder.CreateIndex(
                name: "IX_PayPalPayment_ExecutorId",
                table: "PayPalPayment",
                column: "ExecutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Performers_OrganizationAddressId",
                table: "Performers",
                column: "OrganizationAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_ActivityCode",
                table: "Project",
                column: "ActivityCode");

            migrationBuilder.CreateIndex(
                name: "IX_Project_ClientId",
                table: "Project",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_GitId",
                table: "Project",
                column: "GitId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_PaymentId",
                table: "Project",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_PerformerId",
                table: "Project",
                column: "PerformerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBuildConfiguration_ProjectId",
                table: "ProjectBuildConfiguration",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RdvQueries_ActivityCode",
                table: "RdvQueries",
                column: "ActivityCode");

            migrationBuilder.CreateIndex(
                name: "IX_RdvQueries_ClientId",
                table: "RdvQueries",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_RdvQueries_LocationId",
                table: "RdvQueries",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_RdvQueries_PaymentId",
                table: "RdvQueries",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_RdvQueries_PerformerId",
                table: "RdvQueries",
                column: "PerformerId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEvent_PeriodStart_PeriodEnd",
                table: "ScheduledEvent",
                columns: new[] { "PeriodStart", "PeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEvent_ScheduleOwnerId",
                table: "ScheduledEvent",
                column: "ScheduleOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ContextId",
                table: "Services",
                column: "ContextId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_UserId",
                table: "UserActivities",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announce");

            migrationBuilder.DropTable(
                name: "ApiResourceClaims");

            migrationBuilder.DropTable(
                name: "ApiResourceProperties");

            migrationBuilder.DropTable(
                name: "ApiResourceScopes");

            migrationBuilder.DropTable(
                name: "ApiResourceSecrets");

            migrationBuilder.DropTable(
                name: "ApiScopeClaims");

            migrationBuilder.DropTable(
                name: "ApiScopeProperties");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BalanceImpact");

            migrationBuilder.DropTable(
                name: "Ban");

            migrationBuilder.DropTable(
                name: "BankIdentity");

            migrationBuilder.DropTable(
                name: "BlackListed");

            migrationBuilder.DropTable(
                name: "blogSpotPublications");

            migrationBuilder.DropTable(
                name: "BlogTag");

            migrationBuilder.DropTable(
                name: "Bug");

            migrationBuilder.DropTable(
                name: "ChatConnection");

            migrationBuilder.DropTable(
                name: "ChatRoomAccess");

            migrationBuilder.DropTable(
                name: "CircleAuthorizationToBlogPost");

            migrationBuilder.DropTable(
                name: "CircleMembers");

            migrationBuilder.DropTable(
                name: "ClientClaims");

            migrationBuilder.DropTable(
                name: "ClientCorsOrigins");

            migrationBuilder.DropTable(
                name: "ClientGrantTypes");

            migrationBuilder.DropTable(
                name: "ClientIdPRestrictions");

            migrationBuilder.DropTable(
                name: "ClientPostLogoutRedirectUris");

            migrationBuilder.DropTable(
                name: "ClientProperties");

            migrationBuilder.DropTable(
                name: "ClientProviderInfo");

            migrationBuilder.DropTable(
                name: "ClientRedirectUris");

            migrationBuilder.DropTable(
                name: "ClientScopes");

            migrationBuilder.DropTable(
                name: "ClientSecrets");

            migrationBuilder.DropTable(
                name: "CommandForm");

            migrationBuilder.DropTable(
                name: "CommandLine");

            migrationBuilder.DropTable(
                name: "Comment");

            migrationBuilder.DropTable(
                name: "Contact");

            migrationBuilder.DropTable(
                name: "CoWorking");

            migrationBuilder.DropTable(
                name: "DeviceDeclaration");

            migrationBuilder.DropTable(
                name: "DeviceFlowCodes");

            migrationBuilder.DropTable(
                name: "DismissClicked");

            migrationBuilder.DropTable(
                name: "ExceptionsSIREN");

            migrationBuilder.DropTable(
                name: "Form");

            migrationBuilder.DropTable(
                name: "HairCutQueries");

            migrationBuilder.DropTable(
                name: "HairPrestationCollectionItem");

            migrationBuilder.DropTable(
                name: "HairTaintInstance");

            migrationBuilder.DropTable(
                name: "HyperLink");

            migrationBuilder.DropTable(
                name: "IdentityResourceClaims");

            migrationBuilder.DropTable(
                name: "IdentityResourceProperties");

            migrationBuilder.DropTable(
                name: "Instrumentation");

            migrationBuilder.DropTable(
                name: "InstrumentRating");

            migrationBuilder.DropTable(
                name: "LiveFlow");

            migrationBuilder.DropTable(
                name: "MailingTemplate");

            migrationBuilder.DropTable(
                name: "MusicalPreference");

            migrationBuilder.DropTable(
                name: "Option");

            migrationBuilder.DropTable(
                name: "PersistedGrants");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "ProjectBuildConfiguration");

            migrationBuilder.DropTable(
                name: "ScheduledEvent");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "SiteSkills");

            migrationBuilder.DropTable(
                name: "UserActivities");

            migrationBuilder.DropTable(
                name: "ApiResources");

            migrationBuilder.DropTable(
                name: "ApiScopes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "BankStatus");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Feature");

            migrationBuilder.DropTable(
                name: "ChatRoom");

            migrationBuilder.DropTable(
                name: "Circle");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "EstimateTemplates");

            migrationBuilder.DropTable(
                name: "Estimates");

            migrationBuilder.DropTable(
                name: "BlogSpot");

            migrationBuilder.DropTable(
                name: "PostalAddress");

            migrationBuilder.DropTable(
                name: "FormationSettings");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "HairMultiCutQueries");

            migrationBuilder.DropTable(
                name: "HairPrestation");

            migrationBuilder.DropTable(
                name: "HairTaint");

            migrationBuilder.DropTable(
                name: "BrusherProfile");

            migrationBuilder.DropTable(
                name: "IdentityResources");

            migrationBuilder.DropTable(
                name: "Instrument");

            migrationBuilder.DropTable(
                name: "DjSettings");

            migrationBuilder.DropTable(
                name: "GeneralSettings");

            migrationBuilder.DropTable(
                name: "MusicalTendency");

            migrationBuilder.DropTable(
                name: "Project");

            migrationBuilder.DropTable(
                name: "Period");

            migrationBuilder.DropTable(
                name: "RdvQueries");

            migrationBuilder.DropTable(
                name: "Color");

            migrationBuilder.DropTable(
                name: "Schedule");

            migrationBuilder.DropTable(
                name: "GitRepositoryReference");

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "PayPalPayment");

            migrationBuilder.DropTable(
                name: "Performers");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
