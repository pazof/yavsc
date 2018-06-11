using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace Yavsc.Migrations
{
    public partial class gitKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    NormalizedName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityRole", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Ban",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false),
                    UserCreated = table.Column<string>(nullable: true),
                    UserModified = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ban", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    DisplayName = table.Column<string>(nullable: true),
                    LogoutRedirectUri = table.Column<string>(nullable: true),
                    RedirectUri = table.Column<string>(nullable: true),
                    RefreshTokenLifeTime = table.Column<int>(nullable: false),
                    Secret = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ClientId = table.Column<string>(nullable: false),
                    ExpiresUtc = table.Column<DateTime>(nullable: false),
                    IssuedUtc = table.Column<DateTime>(nullable: false),
                    ProtectedTicket = table.Column<string>(nullable: false),
                    Subject = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "BankIdentity",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    AccountNumber = table.Column<string>(nullable: true),
                    BIC = table.Column<string>(nullable: true),
                    BankCode = table.Column<string>(nullable: true),
                    BankedKey = table.Column<int>(nullable: false),
                    IBAN = table.Column<string>(nullable: true),
                    WicketCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankIdentity", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "EstimateTemplate",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Description = table.Column<string>(nullable: true),
                    OwnerId = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstimateTemplate", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "ExceptionSIREN",
                columns: table => new
                {
                    SIREN = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionSIREN", x => x.SIREN);
                });
            migrationBuilder.CreateTable(
                name: "Period",
                columns: table => new
                {
                    Start = table.Column<DateTime>(nullable: false),
                    End = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Period", x => new { x.Start, x.End });
                });
            migrationBuilder.CreateTable(
                name: "Option",
                columns: table => new
                {
                    Code = table.Column<string>(nullable: false),
                    CodeScrutin = table.Column<string>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    UserCreated = table.Column<string>(nullable: true),
                    UserModified = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Option", x => new { x.Code, x.CodeScrutin });
                });
            migrationBuilder.CreateTable(
                name: "Color",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Blue = table.Column<byte>(nullable: false),
                    Green = table.Column<byte>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Red = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Color", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Form",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Summary = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Form", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "HairPrestation",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Cares = table.Column<bool>(nullable: false),
                    Cut = table.Column<bool>(nullable: false),
                    Dressing = table.Column<int>(nullable: false),
                    Gender = table.Column<int>(nullable: false),
                    Length = table.Column<int>(nullable: false),
                    Shampoo = table.Column<bool>(nullable: false),
                    Tech = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HairPrestation", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Feature",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Description = table.Column<string>(nullable: true),
                    ShortName = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feature", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Depth = table.Column<decimal>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Height = table.Column<decimal>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Price = table.Column<decimal>(nullable: true),
                    Public = table.Column<bool>(nullable: false),
                    Weight = table.Column<decimal>(nullable: false),
                    Width = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "ClientProviderInfo",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    Avatar = table.Column<string>(nullable: true),
                    BillingAddressId = table.Column<long>(nullable: false),
                    EMail = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    UserName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProviderInfo", x => x.UserId);
                });
            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Target = table.Column<string>(nullable: true),
                    body = table.Column<string>(nullable: false),
                    click_action = table.Column<string>(nullable: false),
                    color = table.Column<string>(nullable: true),
                    icon = table.Column<string>(nullable: true, defaultValue: "exclam"),
                    sound = table.Column<string>(nullable: true),
                    tag = table.Column<string>(nullable: true),
                    title = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Instrument",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrument", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "MusicalTendency",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicalTendency", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "DjSettings",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    SoundCloudId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DjSettings", x => x.UserId);
                });
            migrationBuilder.CreateTable(
                name: "GeneralSettings",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralSettings", x => x.UserId);
                });
            migrationBuilder.CreateTable(
                name: "OAuth2Tokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    AccessToken = table.Column<string>(nullable: true),
                    Expiration = table.Column<DateTime>(nullable: false),
                    ExpiresIn = table.Column<string>(nullable: true),
                    RefreshToken = table.Column<string>(nullable: true),
                    TokenType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuth2Tokens", x => x.UserId);
                });
            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Address = table.Column<string>(nullable: false),
                    Latitude = table.Column<double>(nullable: false),
                    Longitude = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Skill",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Name = table.Column<string>(nullable: true),
                    Rate = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skill", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "Activity",
                columns: table => new
                {
                    Code = table.Column<string>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Hidden = table.Column<bool>(nullable: false),
                    ModeratorGroupName = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    ParentCode = table.Column<string>(nullable: true),
                    Photo = table.Column<string>(nullable: true),
                    Rate = table.Column<int>(nullable: false),
                    SettingsClassName = table.Column<string>(nullable: true),
                    UserCreated = table.Column<string>(nullable: true),
                    UserModified = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activity", x => x.Code);
                    table.ForeignKey(
                        name: "FK_Activity_Activity_ParentCode",
                        column: x => x.ParentCode,
                        principalTable: "Activity",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "FormationSettings",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormationSettings", x => x.UserId);
                });
            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityRoleClaim<string>", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityRoleClaim<string>_IdentityRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "HairTaint",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Brand = table.Column<string>(nullable: true),
                    ColorId = table.Column<long>(nullable: false)
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
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Description = table.Column<string>(nullable: true),
                    FeatureId = table.Column<long>(nullable: true),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bug", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bug_Feature_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Feature",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "MusicalPreference",
                columns: table => new
                {
                    OwnerProfileId = table.Column<string>(nullable: false),
                    DjSettingsUserId = table.Column<string>(nullable: true),
                    GeneralSettingsUserId = table.Column<string>(nullable: true),
                    Rate = table.Column<int>(nullable: false),
                    TendencyId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicalPreference", x => x.OwnerProfileId);
                    table.ForeignKey(
                        name: "FK_MusicalPreference_DjSettings_DjSettingsUserId",
                        column: x => x.DjSettingsUserId,
                        principalTable: "DjSettings",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MusicalPreference_GeneralSettings_GeneralSettingsUserId",
                        column: x => x.GeneralSettingsUserId,
                        principalTable: "GeneralSettings",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    AllowMonthlyEmail = table.Column<bool>(nullable: false),
                    Avatar = table.Column<string>(nullable: true, defaultValue: "/images/Users/icon_user.png"),
                    BankInfoId = table.Column<long>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    DedicatedGoogleCalendar = table.Column<string>(nullable: true),
                    DiskQuota = table.Column<long>(nullable: false, defaultValue: 524288000L),
                    DiskUsage = table.Column<long>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    FullName = table.Column<string>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    MaxFileSize = table.Column<long>(nullable: false),
                    NormalizedEmail = table.Column<string>(nullable: true),
                    NormalizedUserName = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    PostalAddressId = table.Column<long>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    UserName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationUser_BankIdentity_BankInfoId",
                        column: x => x.BankInfoId,
                        principalTable: "BankIdentity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationUser_Location_PostalAddressId",
                        column: x => x.PostalAddressId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "Service",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    ContextId = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Public = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Service", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Service_Activity_ContextId",
                        column: x => x.ContextId,
                        principalTable: "Activity",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "CommandForm",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    ActionName = table.Column<string>(nullable: true),
                    ActivityCode = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandForm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommandForm_Activity_ActivityCode",
                        column: x => x.ActivityCode,
                        principalTable: "Activity",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "HairTaintInstance",
                columns: table => new
                {
                    TaintId = table.Column<long>(nullable: false),
                    PrestationId = table.Column<long>(nullable: false)
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
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityUserClaim<string>", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityUserClaim<string>_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityUserLogin<string>", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_IdentityUserLogin<string>_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityUserRole<string>", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_IdentityUserRole<string>_IdentityRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IdentityUserRole<string>_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "BlackListed",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    OwnerId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlackListed", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlackListed_ApplicationUser_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "AccountBalance",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    ContactCredits = table.Column<long>(nullable: false),
                    Credits = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBalance", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_AccountBalance_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "BlogPost",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    AuthorId = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false),
                    Photo = table.Column<string>(nullable: true),
                    Rate = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    UserCreated = table.Column<string>(nullable: true),
                    UserModified = table.Column<string>(nullable: true),
                    Visible = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPost", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogPost_ApplicationUser_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "Schedule",
                columns: table => new
                {
                    OwnerId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule", x => x.OwnerId);
                    table.ForeignKey(
                        name: "FK_Schedule_ApplicationUser_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "ChatConnection",
                columns: table => new
                {
                    ConnectionId = table.Column<string>(nullable: false),
                    ApplicationUserId = table.Column<string>(nullable: false),
                    Connected = table.Column<bool>(nullable: false),
                    UserAgent = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatConnection", x => x.ConnectionId);
                    table.ForeignKey(
                        name: "FK_ChatConnection_ApplicationUser_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "ChatRoom",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    ApplicationUserId = table.Column<string>(nullable: true),
                    Topic = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRoom", x => x.Name);
                    table.ForeignKey(
                        name: "FK_ChatRoom_ApplicationUser_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "GoogleCloudMobileDeclaration",
                columns: table => new
                {
                    DeviceId = table.Column<string>(nullable: false),
                    DeclarationDate = table.Column<DateTime>(nullable: false, defaultValueSql: "LOCALTIMESTAMP"),
                    DeviceOwnerId = table.Column<string>(nullable: true),
                    GCMRegistrationId = table.Column<string>(nullable: false),
                    LatestActivityUpdate = table.Column<DateTime>(nullable: false),
                    Model = table.Column<string>(nullable: true),
                    Platform = table.Column<string>(nullable: true),
                    Version = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleCloudMobileDeclaration", x => x.DeviceId);
                    table.ForeignKey(
                        name: "FK_GoogleCloudMobileDeclaration_ApplicationUser_DeviceOwnerId",
                        column: x => x.DeviceOwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "Announce",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    For = table.Column<byte>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    OwnerId = table.Column<string>(nullable: true),
                    Sender = table.Column<string>(nullable: true),
                    Topic = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announce", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Announce_ApplicationUser_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "DimissClicked",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    NotificationId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DimissClicked", x => new { x.UserId, x.NotificationId });
                    table.ForeignKey(
                        name: "FK_DimissClicked_Notification_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DimissClicked_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "PayPalPayment",
                columns: table => new
                {
                    CreationToken = table.Column<string>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false),
                    ExecutorId = table.Column<string>(nullable: false),
                    OrderReference = table.Column<string>(nullable: true),
                    PaypalPayerId = table.Column<string>(nullable: true),
                    State = table.Column<string>(nullable: true),
                    UserCreated = table.Column<string>(nullable: true),
                    UserModified = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayPalPayment", x => x.CreationToken);
                    table.ForeignKey(
                        name: "FK_PayPalPayment_ApplicationUser_ExecutorId",
                        column: x => x.ExecutorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "Circle",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    ApplicationUserId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    OwnerId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Circle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Circle_ApplicationUser_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    OwnerId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    ApplicationUserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => new { x.OwnerId, x.UserId });
                    table.ForeignKey(
                        name: "FK_Contact_ApplicationUser_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "PerformerProfile",
                columns: table => new
                {
                    PerformerId = table.Column<string>(nullable: false),
                    AcceptNotifications = table.Column<bool>(nullable: false),
                    AcceptPublicContact = table.Column<bool>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    MaxDailyCost = table.Column<int>(nullable: true),
                    MinDailyCost = table.Column<int>(nullable: true),
                    OrganizationAddressId = table.Column<long>(nullable: false),
                    Rate = table.Column<int>(nullable: false),
                    SIREN = table.Column<string>(nullable: false),
                    UseGeoLocalizationToReduceDistanceWithClients = table.Column<bool>(nullable: false),
                    WebSite = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformerProfile", x => x.PerformerId);
                    table.ForeignKey(
                        name: "FK_PerformerProfile_Location_OrganizationAddressId",
                        column: x => x.OrganizationAddressId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerformerProfile_ApplicationUser_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "MailingTemplate",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Body = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false),
                    ManagerId = table.Column<string>(nullable: true),
                    ReplyToAddress = table.Column<string>(nullable: true),
                    ToSend = table.Column<int>(nullable: false),
                    Topic = table.Column<string>(nullable: true),
                    UserCreated = table.Column<string>(nullable: true),
                    UserModified = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailingTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MailingTemplate_ApplicationUser_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "GitRepositoryReference",
                columns: table => new
                {
                    Path = table.Column<string>(nullable: false),
                    Branch = table.Column<string>(nullable: true),
                    OwnerId = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitRepositoryReference", x => x.Path);
                    table.ForeignKey(
                        name: "FK_GitRepositoryReference_ApplicationUser_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "BalanceImpact",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    BalanceId = table.Column<string>(nullable: false),
                    ExecDate = table.Column<DateTime>(nullable: false),
                    Impact = table.Column<decimal>(nullable: false),
                    Reason = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceImpact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BalanceImpact_AccountBalance_BalanceId",
                        column: x => x.BalanceId,
                        principalTable: "AccountBalance",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "BlogTag",
                columns: table => new
                {
                    PostId = table.Column<long>(nullable: false),
                    TagId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogTag", x => new { x.PostId, x.TagId });
                    table.ForeignKey(
                        name: "FK_BlogTag_BlogPost_PostId",
                        column: x => x.PostId,
                        principalTable: "BlogPost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogTag_Tag_TagId",
                        column: x => x.TagId,
                        principalTable: "Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    AuthorId = table.Column<string>(nullable: false),
                    Content = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false),
                    ParentId = table.Column<long>(nullable: true),
                    PostId = table.Column<long>(nullable: false),
                    UserCreated = table.Column<string>(nullable: true),
                    UserModified = table.Column<string>(nullable: true),
                    Visible = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comment_ApplicationUser_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comment_Comment_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Comment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comment_BlogPost_PostId",
                        column: x => x.PostId,
                        principalTable: "BlogPost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "ScheduledEvent",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    PeriodEnd = table.Column<DateTime>(nullable: true),
                    PeriodStart = table.Column<DateTime>(nullable: true),
                    Reccurence = table.Column<int>(nullable: false),
                    ScheduleOwnerId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledEvent_Schedule_ScheduleOwnerId",
                        column: x => x.ScheduleOwnerId,
                        principalTable: "Schedule",
                        principalColumn: "OwnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledEvent_Period_PeriodStart_PeriodEnd",
                        columns: x => new { x.PeriodStart, x.PeriodEnd },
                        principalTable: "Period",
                        principalColumns: new[] { "Start", "End" },
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "ChatRoomPresence",
                columns: table => new
                {
                    ChannelName = table.Column<string>(nullable: false),
                    ChatUserConnectionId = table.Column<string>(nullable: false),
                    Level = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRoomPresence", x => new { x.ChannelName, x.ChatUserConnectionId });
                    table.ForeignKey(
                        name: "FK_ChatRoomPresence_ChatRoom_ChannelName",
                        column: x => x.ChannelName,
                        principalTable: "ChatRoom",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatRoomPresence_ChatConnection_ChatUserConnectionId",
                        column: x => x.ChatUserConnectionId,
                        principalTable: "ChatConnection",
                        principalColumn: "ConnectionId",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "CircleAuthorizationToBlogPost",
                columns: table => new
                {
                    CircleId = table.Column<long>(nullable: false),
                    BlogPostId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleAuthorizationToBlogPost", x => new { x.CircleId, x.BlogPostId });
                    table.ForeignKey(
                        name: "FK_CircleAuthorizationToBlogPost_BlogPost_BlogPostId",
                        column: x => x.BlogPostId,
                        principalTable: "BlogPost",
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
                name: "CircleMember",
                columns: table => new
                {
                    MemberId = table.Column<string>(nullable: false),
                    CircleId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleMember", x => new { x.MemberId, x.CircleId });
                    table.ForeignKey(
                        name: "FK_CircleMember_Circle_CircleId",
                        column: x => x.CircleId,
                        principalTable: "Circle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CircleMember_ApplicationUser_MemberId",
                        column: x => x.MemberId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "BrusherProfile",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    ActionDistance = table.Column<int>(nullable: false),
                    CarePrice = table.Column<decimal>(nullable: false),
                    FlatFeeDiscount = table.Column<decimal>(nullable: false),
                    HalfBalayagePrice = table.Column<decimal>(nullable: false),
                    HalfBrushingPrice = table.Column<decimal>(nullable: false),
                    HalfColorPrice = table.Column<decimal>(nullable: false),
                    HalfDefrisPrice = table.Column<decimal>(nullable: false),
                    HalfFoldingPrice = table.Column<decimal>(nullable: false),
                    HalfMechPrice = table.Column<decimal>(nullable: false),
                    HalfMultiColorPrice = table.Column<decimal>(nullable: false),
                    HalfPermanentPrice = table.Column<decimal>(nullable: false),
                    KidCutPrice = table.Column<decimal>(nullable: false),
                    LongBalayagePrice = table.Column<decimal>(nullable: false),
                    LongBrushingPrice = table.Column<decimal>(nullable: false),
                    LongColorPrice = table.Column<decimal>(nullable: false),
                    LongDefrisPrice = table.Column<decimal>(nullable: false),
                    LongFoldingPrice = table.Column<decimal>(nullable: false),
                    LongMechPrice = table.Column<decimal>(nullable: false),
                    LongMultiColorPrice = table.Column<decimal>(nullable: false),
                    LongPermanentPrice = table.Column<decimal>(nullable: false),
                    ManBrushPrice = table.Column<decimal>(nullable: false),
                    ManCutPrice = table.Column<decimal>(nullable: false),
                    ScheduleOwnerId = table.Column<string>(nullable: true),
                    ShampooPrice = table.Column<decimal>(nullable: false),
                    ShortBalayagePrice = table.Column<decimal>(nullable: false),
                    ShortBrushingPrice = table.Column<decimal>(nullable: false),
                    ShortColorPrice = table.Column<decimal>(nullable: false),
                    ShortDefrisPrice = table.Column<decimal>(nullable: false),
                    ShortFoldingPrice = table.Column<decimal>(nullable: false),
                    ShortMechPrice = table.Column<decimal>(nullable: false),
                    ShortMultiColorPrice = table.Column<decimal>(nullable: false),
                    ShortPermanentPrice = table.Column<decimal>(nullable: false),
                    WomenHalfCutPrice = table.Column<decimal>(nullable: false),
                    WomenLongCutPrice = table.Column<decimal>(nullable: false),
                    WomenShortCutPrice = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrusherProfile", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_BrusherProfile_Schedule_ScheduleOwnerId",
                        column: x => x.ScheduleOwnerId,
                        principalTable: "Schedule",
                        principalColumn: "OwnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BrusherProfile_PerformerProfile_UserId",
                        column: x => x.UserId,
                        principalTable: "PerformerProfile",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "HairMultiCutQuery",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    ActivityCode = table.Column<string>(nullable: false),
                    ClientId = table.Column<string>(nullable: false),
                    Consent = table.Column<bool>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false),
                    EventDate = table.Column<DateTime>(nullable: false),
                    LocationId = table.Column<long>(nullable: true),
                    PaymentId = table.Column<string>(nullable: true),
                    PerformerId = table.Column<string>(nullable: false),
                    Previsional = table.Column<decimal>(nullable: true),
                    Rejected = table.Column<bool>(nullable: false),
                    RejectedAt = table.Column<DateTime>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    UserCreated = table.Column<string>(nullable: true),
                    UserModified = table.Column<string>(nullable: true),
                    ValidationDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HairMultiCutQuery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HairMultiCutQuery_Activity_ActivityCode",
                        column: x => x.ActivityCode,
                        principalTable: "Activity",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairMultiCutQuery_ApplicationUser_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairMultiCutQuery_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HairMultiCutQuery_PayPalPayment_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "PayPalPayment",
                        principalColumn: "CreationToken",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HairMultiCutQuery_PerformerProfile_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "PerformerProfile",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "Instrumentation",
                columns: table => new
                {
                    InstrumentId = table.Column<long>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
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
                        name: "FK_Instrumentation_PerformerProfile_UserId",
                        column: x => x.UserId,
                        principalTable: "PerformerProfile",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "CoWorking",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    FormationSettingsUserId = table.Column<string>(nullable: true),
                    PerformerId = table.Column<string>(nullable: true),
                    WorkingForId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoWorking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoWorking_FormationSettings_FormationSettingsUserId",
                        column: x => x.FormationSettingsUserId,
                        principalTable: "FormationSettings",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoWorking_PerformerProfile_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "PerformerProfile",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoWorking_ApplicationUser_WorkingForId",
                        column: x => x.WorkingForId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "RdvQuery",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    ActivityCode = table.Column<string>(nullable: false),
                    ClientId = table.Column<string>(nullable: false),
                    Consent = table.Column<bool>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false),
                    EventDate = table.Column<DateTime>(nullable: false),
                    LocationId = table.Column<long>(nullable: true),
                    LocationType = table.Column<int>(nullable: false),
                    PaymentId = table.Column<string>(nullable: true),
                    PerformerId = table.Column<string>(nullable: false),
                    Previsional = table.Column<decimal>(nullable: true),
                    Reason = table.Column<string>(nullable: true),
                    Rejected = table.Column<bool>(nullable: false),
                    RejectedAt = table.Column<DateTime>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    UserCreated = table.Column<string>(nullable: true),
                    UserModified = table.Column<string>(nullable: true),
                    ValidationDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RdvQuery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RdvQuery_Activity_ActivityCode",
                        column: x => x.ActivityCode,
                        principalTable: "Activity",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RdvQuery_ApplicationUser_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RdvQuery_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RdvQuery_PayPalPayment_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "PayPalPayment",
                        principalColumn: "CreationToken",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RdvQuery_PerformerProfile_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "PerformerProfile",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "UserActivity",
                columns: table => new
                {
                    DoesCode = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    Weight = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivity", x => new { x.DoesCode, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserActivity_Activity_DoesCode",
                        column: x => x.DoesCode,
                        principalTable: "Activity",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserActivity_PerformerProfile_UserId",
                        column: x => x.UserId,
                        principalTable: "PerformerProfile",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "HairCutQuery",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    ActivityCode = table.Column<string>(nullable: false),
                    AdditionalInfo = table.Column<string>(nullable: true),
                    ClientId = table.Column<string>(nullable: false),
                    Consent = table.Column<bool>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false),
                    EventDate = table.Column<DateTime>(nullable: true),
                    LocationId = table.Column<long>(nullable: true),
                    PaymentId = table.Column<string>(nullable: true),
                    PerformerId = table.Column<string>(nullable: false),
                    PrestationId = table.Column<long>(nullable: false),
                    Previsional = table.Column<decimal>(nullable: true),
                    Rejected = table.Column<bool>(nullable: false),
                    RejectedAt = table.Column<DateTime>(nullable: false),
                    SelectedProfileUserId = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    UserCreated = table.Column<string>(nullable: true),
                    UserModified = table.Column<string>(nullable: true),
                    ValidationDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HairCutQuery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HairCutQuery_Activity_ActivityCode",
                        column: x => x.ActivityCode,
                        principalTable: "Activity",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairCutQuery_ApplicationUser_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairCutQuery_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HairCutQuery_PayPalPayment_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "PayPalPayment",
                        principalColumn: "CreationToken",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HairCutQuery_PerformerProfile_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "PerformerProfile",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairCutQuery_HairPrestation_PrestationId",
                        column: x => x.PrestationId,
                        principalTable: "HairPrestation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairCutQuery_BrusherProfile_SelectedProfileUserId",
                        column: x => x.SelectedProfileUserId,
                        principalTable: "BrusherProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "HyperLink",
                columns: table => new
                {
                    HRef = table.Column<string>(nullable: false),
                    Method = table.Column<string>(nullable: false),
                    BrusherProfileUserId = table.Column<string>(nullable: true),
                    ContentType = table.Column<string>(nullable: true),
                    PayPalPaymentCreationToken = table.Column<string>(nullable: true),
                    Rel = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HyperLink", x => new { x.HRef, x.Method });
                    table.ForeignKey(
                        name: "FK_HyperLink_BrusherProfile_BrusherProfileUserId",
                        column: x => x.BrusherProfileUserId,
                        principalTable: "BrusherProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HyperLink_PayPalPayment_PayPalPaymentCreationToken",
                        column: x => x.PayPalPaymentCreationToken,
                        principalTable: "PayPalPayment",
                        principalColumn: "CreationToken",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "HairPrestationCollectionItem",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    PrestationId = table.Column<long>(nullable: false),
                    QueryId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HairPrestationCollectionItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HairPrestationCollectionItem_HairPrestation_PrestationId",
                        column: x => x.PrestationId,
                        principalTable: "HairPrestation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairPrestationCollectionItem_HairMultiCutQuery_QueryId",
                        column: x => x.QueryId,
                        principalTable: "HairMultiCutQuery",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "Estimate",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    AttachedFilesString = table.Column<string>(nullable: true),
                    AttachedGraphicsString = table.Column<string>(nullable: true),
                    ClientId = table.Column<string>(nullable: false),
                    ClientValidationDate = table.Column<DateTime>(nullable: false),
                    CommandId = table.Column<long>(nullable: true),
                    CommandType = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    OwnerId = table.Column<string>(nullable: true),
                    ProviderValidationDate = table.Column<DateTime>(nullable: false),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estimate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Estimate_ApplicationUser_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Estimate_RdvQuery_CommandId",
                        column: x => x.CommandId,
                        principalTable: "RdvQuery",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Estimate_PerformerProfile_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "PerformerProfile",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "CommandLine",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Count = table.Column<int>(nullable: false),
                    Currency = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: false),
                    EstimateId = table.Column<long>(nullable: false),
                    EstimateTemplateId = table.Column<long>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    UnitaryCost = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommandLine_Estimate_EstimateId",
                        column: x => x.EstimateId,
                        principalTable: "Estimate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommandLine_EstimateTemplate_EstimateTemplateId",
                        column: x => x.EstimateTemplateId,
                        principalTable: "EstimateTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName");
            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");
            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("AspNetRoleClaims");
            migrationBuilder.DropTable("AspNetUserClaims");
            migrationBuilder.DropTable("AspNetUserLogins");
            migrationBuilder.DropTable("AspNetUserRoles");
            migrationBuilder.DropTable("Ban");
            migrationBuilder.DropTable("BlackListed");
            migrationBuilder.DropTable("CircleAuthorizationToBlogPost");
            migrationBuilder.DropTable("Client");
            migrationBuilder.DropTable("RefreshToken");
            migrationBuilder.DropTable("BalanceImpact");
            migrationBuilder.DropTable("CommandLine");
            migrationBuilder.DropTable("ExceptionSIREN");
            migrationBuilder.DropTable("BlogTag");
            migrationBuilder.DropTable("Comment");
            migrationBuilder.DropTable("ScheduledEvent");
            migrationBuilder.DropTable("ChatRoomPresence");
            migrationBuilder.DropTable("Option");
            migrationBuilder.DropTable("Form");
            migrationBuilder.DropTable("HairCutQuery");
            migrationBuilder.DropTable("HairPrestationCollectionItem");
            migrationBuilder.DropTable("HairTaintInstance");
            migrationBuilder.DropTable("GoogleCloudMobileDeclaration");
            migrationBuilder.DropTable("Bug");
            migrationBuilder.DropTable("Product");
            migrationBuilder.DropTable("Service");
            migrationBuilder.DropTable("Announce");
            migrationBuilder.DropTable("ClientProviderInfo");
            migrationBuilder.DropTable("DimissClicked");
            migrationBuilder.DropTable("MusicalPreference");
            migrationBuilder.DropTable("MusicalTendency");
            migrationBuilder.DropTable("Instrumentation");
            migrationBuilder.DropTable("OAuth2Tokens");
            migrationBuilder.DropTable("CircleMember");
            migrationBuilder.DropTable("Contact");
            migrationBuilder.DropTable("HyperLink");
            migrationBuilder.DropTable("Skill");
            migrationBuilder.DropTable("CommandForm");
            migrationBuilder.DropTable("CoWorking");
            migrationBuilder.DropTable("UserActivity");
            migrationBuilder.DropTable("MailingTemplate");
            migrationBuilder.DropTable("GitRepositoryReference");
            migrationBuilder.DropTable("AspNetRoles");
            migrationBuilder.DropTable("AccountBalance");
            migrationBuilder.DropTable("Estimate");
            migrationBuilder.DropTable("EstimateTemplate");
            migrationBuilder.DropTable("Tag");
            migrationBuilder.DropTable("BlogPost");
            migrationBuilder.DropTable("Period");
            migrationBuilder.DropTable("ChatRoom");
            migrationBuilder.DropTable("ChatConnection");
            migrationBuilder.DropTable("HairMultiCutQuery");
            migrationBuilder.DropTable("HairPrestation");
            migrationBuilder.DropTable("HairTaint");
            migrationBuilder.DropTable("Feature");
            migrationBuilder.DropTable("Notification");
            migrationBuilder.DropTable("DjSettings");
            migrationBuilder.DropTable("GeneralSettings");
            migrationBuilder.DropTable("Instrument");
            migrationBuilder.DropTable("Circle");
            migrationBuilder.DropTable("BrusherProfile");
            migrationBuilder.DropTable("FormationSettings");
            migrationBuilder.DropTable("RdvQuery");
            migrationBuilder.DropTable("Color");
            migrationBuilder.DropTable("Schedule");
            migrationBuilder.DropTable("Activity");
            migrationBuilder.DropTable("PayPalPayment");
            migrationBuilder.DropTable("PerformerProfile");
            migrationBuilder.DropTable("AspNetUsers");
            migrationBuilder.DropTable("BankIdentity");
            migrationBuilder.DropTable("Location");
        }
    }
}
