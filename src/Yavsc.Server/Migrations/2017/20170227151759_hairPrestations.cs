
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Yavsc.Migrations
{
    public partial class hairPrestations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_IdentityRoleClaim<string>_IdentityRole_RoleId", table: "AspNetRoleClaims");
            migrationBuilder.DropForeignKey(name: "FK_IdentityUserClaim<string>_ApplicationUser_UserId", table: "AspNetUserClaims");
            migrationBuilder.DropForeignKey(name: "FK_IdentityUserLogin<string>_ApplicationUser_UserId", table: "AspNetUserLogins");
            migrationBuilder.DropForeignKey(name: "FK_IdentityUserRole<string>_IdentityRole_RoleId", table: "AspNetUserRoles");
            migrationBuilder.DropForeignKey(name: "FK_IdentityUserRole<string>_ApplicationUser_UserId", table: "AspNetUserRoles");
            migrationBuilder.DropForeignKey(name: "FK_BlackListed_ApplicationUser_OwnerId", table: "BlackListed");
            migrationBuilder.DropForeignKey(name: "FK_CircleAuthorizationToBlogPost_Blog_BlogPostId", table: "CircleAuthorizationToBlogPost");
            migrationBuilder.DropForeignKey(name: "FK_CircleAuthorizationToBlogPost_Circle_CircleId", table: "CircleAuthorizationToBlogPost");
            migrationBuilder.DropForeignKey(name: "FK_AccountBalance_ApplicationUser_UserId", table: "AccountBalance");
            migrationBuilder.DropForeignKey(name: "FK_BalanceImpact_AccountBalance_BalanceId", table: "BalanceImpact");
            migrationBuilder.DropForeignKey(name: "FK_CommandLine_BaseProduct_ArticleId", table: "CommandLine");
            migrationBuilder.DropForeignKey(name: "FK_CommandLine_Estimate_EstimateId", table: "CommandLine");
            migrationBuilder.DropForeignKey(name: "FK_Estimate_ApplicationUser_ClientId", table: "Estimate");
            migrationBuilder.DropForeignKey(name: "FK_Estimate_BookQuery_CommandId", table: "Estimate");
            migrationBuilder.DropForeignKey(name: "FK_Estimate_PerformerProfile_OwnerId", table: "Estimate");
            migrationBuilder.DropForeignKey(name: "FK_HairTaint_Color_ColorId", table: "HairTaint");
            migrationBuilder.DropForeignKey(name: "FK_DimissClicked_Notification_NotificationId", table: "DimissClicked");
            migrationBuilder.DropForeignKey(name: "FK_DimissClicked_ApplicationUser_UserId", table: "DimissClicked");
            migrationBuilder.DropForeignKey(name: "FK_Instrumentation_Instrument_InstrumentId", table: "Instrumentation");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_Circle_CircleId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_ApplicationUser_MemberId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_PostTag_Blog_PostId", table: "PostTag");
            migrationBuilder.DropForeignKey(name: "FK_CommandForm_Activity_ActivityCode", table: "CommandForm");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Location_OrganizationAddressId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_ApplicationUser_PerformerId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_UserActivity_Activity_DoesCode", table: "UserActivity");
            migrationBuilder.DropForeignKey(name: "FK_UserActivity_PerformerProfile_UserId", table: "UserActivity");
            migrationBuilder.DropColumn(name: "ArticleId", table: "CommandLine");
            migrationBuilder.DropTable("BaseProduct");
            migrationBuilder.DropTable("BookQuery");
            // les id de requete existant venaient d'une table nomée "BookQuery" 
            // qui n'existe plus.
            migrationBuilder.Sql("DELETE FROM \"Estimate\"");
            migrationBuilder.Sql("DELETE FROM \"CommandLine\"");
            migrationBuilder.CreateTable(
                name: "HairMultiCutQuery",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    ActivityCode = table.Column<string>(nullable: false),
                    ClientId = table.Column<string>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false),
                    EventDate = table.Column<DateTime>(nullable: false),
                    LocationId = table.Column<long>(nullable: true),
                    PerformerId = table.Column<string>(nullable: false),
                    Previsional = table.Column<decimal>(nullable: true),
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
                        name: "FK_HairMultiCutQuery_PerformerProfile_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "PerformerProfile",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
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
                name: "RdvQuery",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    ActivityCode = table.Column<string>(nullable: false),
                    ClientId = table.Column<string>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false),
                    EventDate = table.Column<DateTime>(nullable: false),
                    LocationId = table.Column<long>(nullable: true),
                    LocationTypeId = table.Column<long>(nullable: true),
                    PerformerId = table.Column<string>(nullable: false),
                    Previsional = table.Column<decimal>(nullable: true),
                    Reason = table.Column<string>(nullable: true),
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
                        name: "FK_RdvQuery_LocationType_LocationTypeId",
                        column: x => x.LocationTypeId,
                        principalTable: "LocationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RdvQuery_PerformerProfile_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "PerformerProfile",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
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
                    HairMultiCutQueryId = table.Column<long>(nullable: true),
                    Length = table.Column<int>(nullable: false),
                    Shampoo = table.Column<bool>(nullable: false),
                    Tech = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HairPrestation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HairPrestation_HairMultiCutQuery_HairMultiCutQueryId",
                        column: x => x.HairMultiCutQueryId,
                        principalTable: "HairMultiCutQuery",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateTable(
                name: "HairCutQuery",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    ActivityCode = table.Column<string>(nullable: false),
                    ClientId = table.Column<string>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false),
                    EventDate = table.Column<DateTime>(nullable: false),
                    LocationId = table.Column<long>(nullable: true),
                    PerformerId = table.Column<string>(nullable: false),
                    PrestationId = table.Column<long>(nullable: true),
                    Previsional = table.Column<decimal>(nullable: true),
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
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.AddColumn<long>(
                name: "HairPrestationId",
                table: "HairTaint",
                nullable: true);
            migrationBuilder.AddForeignKey(
                name: "FK_IdentityRoleClaim<string>_IdentityRole_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUserClaim<string>_ApplicationUser_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUserLogin<string>_ApplicationUser_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUserRole<string>_IdentityRole_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUserRole<string>_ApplicationUser_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_BlackListed_ApplicationUser_OwnerId",
                table: "BlackListed",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_CircleAuthorizationToBlogPost_Blog_BlogPostId",
                table: "CircleAuthorizationToBlogPost",
                column: "BlogPostId",
                principalTable: "Blog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_CircleAuthorizationToBlogPost_Circle_CircleId",
                table: "CircleAuthorizationToBlogPost",
                column: "CircleId",
                principalTable: "Circle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_AccountBalance_ApplicationUser_UserId",
                table: "AccountBalance",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_BalanceImpact_AccountBalance_BalanceId",
                table: "BalanceImpact",
                column: "BalanceId",
                principalTable: "AccountBalance",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_CommandLine_Estimate_EstimateId",
                table: "CommandLine",
                column: "EstimateId",
                principalTable: "Estimate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_Estimate_ApplicationUser_ClientId",
                table: "Estimate",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_Estimate_RdvQuery_CommandId",
                table: "Estimate",
                column: "CommandId",
                principalTable: "RdvQuery",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Estimate_PerformerProfile_OwnerId",
                table: "Estimate",
                column: "OwnerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_HairTaint_Color_ColorId",
                table: "HairTaint",
                column: "ColorId",
                principalTable: "Color",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_HairTaint_HairPrestation_HairPrestationId",
                table: "HairTaint",
                column: "HairPrestationId",
                principalTable: "HairPrestation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_DimissClicked_Notification_NotificationId",
                table: "DimissClicked",
                column: "NotificationId",
                principalTable: "Notification",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_DimissClicked_ApplicationUser_UserId",
                table: "DimissClicked",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_Instrumentation_Instrument_InstrumentId",
                table: "Instrumentation",
                column: "InstrumentId",
                principalTable: "Instrument",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_CircleMember_Circle_CircleId",
                table: "CircleMember",
                column: "CircleId",
                principalTable: "Circle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_CircleMember_ApplicationUser_MemberId",
                table: "CircleMember",
                column: "MemberId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_PostTag_Blog_PostId",
                table: "PostTag",
                column: "PostId",
                principalTable: "Blog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_CommandForm_Activity_ActivityCode",
                table: "CommandForm",
                column: "ActivityCode",
                principalTable: "Activity",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_PerformerProfile_Location_OrganizationAddressId",
                table: "PerformerProfile",
                column: "OrganizationAddressId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_PerformerProfile_ApplicationUser_PerformerId",
                table: "PerformerProfile",
                column: "PerformerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_UserActivity_Activity_DoesCode",
                table: "UserActivity",
                column: "DoesCode",
                principalTable: "Activity",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_UserActivity_PerformerProfile_UserId",
                table: "UserActivity",
                column: "UserId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_IdentityRoleClaim<string>_IdentityRole_RoleId", table: "AspNetRoleClaims");
            migrationBuilder.DropForeignKey(name: "FK_IdentityUserClaim<string>_ApplicationUser_UserId", table: "AspNetUserClaims");
            migrationBuilder.DropForeignKey(name: "FK_IdentityUserLogin<string>_ApplicationUser_UserId", table: "AspNetUserLogins");
            migrationBuilder.DropForeignKey(name: "FK_IdentityUserRole<string>_IdentityRole_RoleId", table: "AspNetUserRoles");
            migrationBuilder.DropForeignKey(name: "FK_IdentityUserRole<string>_ApplicationUser_UserId", table: "AspNetUserRoles");
            migrationBuilder.DropForeignKey(name: "FK_BlackListed_ApplicationUser_OwnerId", table: "BlackListed");
            migrationBuilder.DropForeignKey(name: "FK_CircleAuthorizationToBlogPost_Blog_BlogPostId", table: "CircleAuthorizationToBlogPost");
            migrationBuilder.DropForeignKey(name: "FK_CircleAuthorizationToBlogPost_Circle_CircleId", table: "CircleAuthorizationToBlogPost");
            migrationBuilder.DropForeignKey(name: "FK_AccountBalance_ApplicationUser_UserId", table: "AccountBalance");
            migrationBuilder.DropForeignKey(name: "FK_BalanceImpact_AccountBalance_BalanceId", table: "BalanceImpact");
            migrationBuilder.DropForeignKey(name: "FK_CommandLine_Estimate_EstimateId", table: "CommandLine");
            migrationBuilder.DropForeignKey(name: "FK_Estimate_ApplicationUser_ClientId", table: "Estimate");
            migrationBuilder.DropForeignKey(name: "FK_Estimate_RdvQuery_CommandId", table: "Estimate");
            migrationBuilder.DropForeignKey(name: "FK_Estimate_PerformerProfile_OwnerId", table: "Estimate");
            migrationBuilder.DropForeignKey(name: "FK_HairTaint_Color_ColorId", table: "HairTaint");
            migrationBuilder.DropForeignKey(name: "FK_HairTaint_HairPrestation_HairPrestationId", table: "HairTaint");
            migrationBuilder.DropForeignKey(name: "FK_DimissClicked_Notification_NotificationId", table: "DimissClicked");
            migrationBuilder.DropForeignKey(name: "FK_DimissClicked_ApplicationUser_UserId", table: "DimissClicked");
            migrationBuilder.DropForeignKey(name: "FK_Instrumentation_Instrument_InstrumentId", table: "Instrumentation");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_Circle_CircleId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_ApplicationUser_MemberId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_PostTag_Blog_PostId", table: "PostTag");
            migrationBuilder.DropForeignKey(name: "FK_CommandForm_Activity_ActivityCode", table: "CommandForm");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Location_OrganizationAddressId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_ApplicationUser_PerformerId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_UserActivity_Activity_DoesCode", table: "UserActivity");
            migrationBuilder.DropForeignKey(name: "FK_UserActivity_PerformerProfile_UserId", table: "UserActivity");
            migrationBuilder.DropColumn(name: "HairPrestationId", table: "HairTaint");
            migrationBuilder.DropTable("HairCutQuery");
            migrationBuilder.DropTable("Product");
            migrationBuilder.DropTable("RdvQuery");
            migrationBuilder.DropTable("HairPrestation");
            migrationBuilder.DropTable("HairMultiCutQuery");
            migrationBuilder.CreateTable(
                name: "BaseProduct",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Description = table.Column<string>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Public = table.Column<bool>(nullable: false),
                    Depth = table.Column<decimal>(nullable: true),
                    Height = table.Column<decimal>(nullable: true),
                    Price = table.Column<decimal>(nullable: true),
                    Weight = table.Column<decimal>(nullable: true),
                    Width = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseProduct", x => x.Id);
                });
            migrationBuilder.CreateTable(
                name: "BookQuery",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    ActivityCode = table.Column<string>(nullable: false),
                    ClientId = table.Column<string>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false),
                    EventDate = table.Column<DateTime>(nullable: false),
                    LocationId = table.Column<long>(nullable: true),
                    LocationTypeId = table.Column<long>(nullable: true),
                    PerformerId = table.Column<string>(nullable: false),
                    Previsional = table.Column<decimal>(nullable: true),
                    Reason = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    UserCreated = table.Column<string>(nullable: true),
                    UserModified = table.Column<string>(nullable: true),
                    ValidationDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookQuery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookQuery_Activity_ActivityCode",
                        column: x => x.ActivityCode,
                        principalTable: "Activity",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookQuery_ApplicationUser_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookQuery_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookQuery_LocationType_LocationTypeId",
                        column: x => x.LocationTypeId,
                        principalTable: "LocationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookQuery_PerformerProfile_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "PerformerProfile",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.AddColumn<long>(
                name: "ArticleId",
                table: "CommandLine",
                nullable: true);
            migrationBuilder.AddForeignKey(
                name: "FK_IdentityRoleClaim<string>_IdentityRole_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUserClaim<string>_ApplicationUser_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUserLogin<string>_ApplicationUser_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUserRole<string>_IdentityRole_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUserRole<string>_ApplicationUser_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_BlackListed_ApplicationUser_OwnerId",
                table: "BlackListed",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_CircleAuthorizationToBlogPost_Blog_BlogPostId",
                table: "CircleAuthorizationToBlogPost",
                column: "BlogPostId",
                principalTable: "Blog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_CircleAuthorizationToBlogPost_Circle_CircleId",
                table: "CircleAuthorizationToBlogPost",
                column: "CircleId",
                principalTable: "Circle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_AccountBalance_ApplicationUser_UserId",
                table: "AccountBalance",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_BalanceImpact_AccountBalance_BalanceId",
                table: "BalanceImpact",
                column: "BalanceId",
                principalTable: "AccountBalance",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_CommandLine_BaseProduct_ArticleId",
                table: "CommandLine",
                column: "ArticleId",
                principalTable: "BaseProduct",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_CommandLine_Estimate_EstimateId",
                table: "CommandLine",
                column: "EstimateId",
                principalTable: "Estimate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Estimate_ApplicationUser_ClientId",
                table: "Estimate",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Estimate_BookQuery_CommandId",
                table: "Estimate",
                column: "CommandId",
                principalTable: "BookQuery",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Estimate_PerformerProfile_OwnerId",
                table: "Estimate",
                column: "OwnerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_HairTaint_Color_ColorId",
                table: "HairTaint",
                column: "ColorId",
                principalTable: "Color",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_DimissClicked_Notification_NotificationId",
                table: "DimissClicked",
                column: "NotificationId",
                principalTable: "Notification",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_DimissClicked_ApplicationUser_UserId",
                table: "DimissClicked",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Instrumentation_Instrument_InstrumentId",
                table: "Instrumentation",
                column: "InstrumentId",
                principalTable: "Instrument",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_CircleMember_Circle_CircleId",
                table: "CircleMember",
                column: "CircleId",
                principalTable: "Circle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_CircleMember_ApplicationUser_MemberId",
                table: "CircleMember",
                column: "MemberId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_PostTag_Blog_PostId",
                table: "PostTag",
                column: "PostId",
                principalTable: "Blog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_CommandForm_Activity_ActivityCode",
                table: "CommandForm",
                column: "ActivityCode",
                principalTable: "Activity",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_PerformerProfile_Location_OrganizationAddressId",
                table: "PerformerProfile",
                column: "OrganizationAddressId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_PerformerProfile_ApplicationUser_PerformerId",
                table: "PerformerProfile",
                column: "PerformerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_UserActivity_Activity_DoesCode",
                table: "UserActivity",
                column: "DoesCode",
                principalTable: "Activity",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_UserActivity_PerformerProfile_UserId",
                table: "UserActivity",
                column: "UserId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
