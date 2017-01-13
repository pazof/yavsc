using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace Yavsc.Migrations
{
    public partial class SettingsClassName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_IdentityRoleClaim<string>_IdentityRole_RoleId", table: "AspNetRoleClaims");
            migrationBuilder.DropForeignKey(name: "FK_IdentityUserClaim<string>_ApplicationUser_UserId", table: "AspNetUserClaims");
            migrationBuilder.DropForeignKey(name: "FK_IdentityUserLogin<string>_ApplicationUser_UserId", table: "AspNetUserLogins");
            migrationBuilder.DropForeignKey(name: "FK_IdentityUserRole<string>_IdentityRole_RoleId", table: "AspNetUserRoles");
            migrationBuilder.DropForeignKey(name: "FK_IdentityUserRole<string>_ApplicationUser_UserId", table: "AspNetUserRoles");
            migrationBuilder.DropForeignKey(name: "FK_AccountBalance_ApplicationUser_UserId", table: "AccountBalance");
            migrationBuilder.DropForeignKey(name: "FK_BalanceImpact_AccountBalance_BalanceId", table: "BalanceImpact");
            migrationBuilder.DropForeignKey(name: "FK_CommandLine_Estimate_EstimateId", table: "CommandLine");
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_ApplicationUser_ClientId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_PerformerProfile_PerformerId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_MusicalPreference_PerformerProfile_OwnerProfileId", table: "MusicalPreference");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_Circle_CircleId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_ApplicationUser_MemberId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_PostTag_Blog_PostId", table: "PostTag");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Service_OfferId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Location_OrganizationAddressId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_ApplicationUser_PerformerId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_UserActivity_Activity_DoesCode", table: "UserActivity");
            migrationBuilder.DropForeignKey(name: "FK_UserActivity_PerformerProfile_UserId", table: "UserActivity");
            migrationBuilder.DropPrimaryKey(name: "PK_UserActivity", table: "UserActivity");
            migrationBuilder.DropPrimaryKey(name: "PK_MusicalPreference", table: "MusicalPreference");
            migrationBuilder.DropColumn(name: "Id", table: "UserActivity");
            migrationBuilder.DropColumn(name: "OfferId", table: "PerformerProfile");
            migrationBuilder.DropColumn(name: "Id", table: "MusicalPreference");
            migrationBuilder.DropColumn(name: "Name", table: "MusicalPreference");
            migrationBuilder.CreateTable(
                name: "MusicianSettings",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicianSettings", x => x.UserId);
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
            migrationBuilder.AddPrimaryKey(
                name: "PK_UserActivity",
                table: "UserActivity",
                columns: new[] { "DoesCode", "UserId" });
            migrationBuilder.AlterColumn<string>(
                name: "OwnerProfileId",
                table: "MusicalPreference",
                nullable: false);
            migrationBuilder.AddColumn<string>(
                name: "DjSettingsUserId",
                table: "MusicalPreference",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "GeneralSettingsUserId",
                table: "MusicalPreference",
                nullable: true);
            migrationBuilder.AddColumn<long>(
                name: "TendencyId",
                table: "MusicalPreference",
                nullable: false,
                defaultValue: 0L);
            migrationBuilder.AddPrimaryKey(
                name: "PK_MusicalPreference",
                table: "MusicalPreference",
                column: "OwnerProfileId");
            migrationBuilder.AddColumn<string>(
                name: "SettingsClassName",
                table: "Activity",
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
                name: "FK_Estimate_PerformerProfile_OwnerId",
                table: "Estimate",
                column: "OwnerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_BookQuery_ApplicationUser_ClientId",
                table: "BookQuery",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_BookQuery_PerformerProfile_PerformerId",
                table: "BookQuery",
                column: "PerformerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_MusicalPreference_DjSettings_DjSettingsUserId",
                table: "MusicalPreference",
                column: "DjSettingsUserId",
                principalTable: "DjSettings",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_MusicalPreference_GeneralSettings_GeneralSettingsUserId",
                table: "MusicalPreference",
                column: "GeneralSettingsUserId",
                principalTable: "GeneralSettings",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
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
            migrationBuilder.DropForeignKey(name: "FK_AccountBalance_ApplicationUser_UserId", table: "AccountBalance");
            migrationBuilder.DropForeignKey(name: "FK_BalanceImpact_AccountBalance_BalanceId", table: "BalanceImpact");
            migrationBuilder.DropForeignKey(name: "FK_CommandLine_Estimate_EstimateId", table: "CommandLine");
            migrationBuilder.DropForeignKey(name: "FK_Estimate_ApplicationUser_ClientId", table: "Estimate");
            migrationBuilder.DropForeignKey(name: "FK_Estimate_PerformerProfile_OwnerId", table: "Estimate");
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_ApplicationUser_ClientId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_PerformerProfile_PerformerId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_MusicalPreference_DjSettings_DjSettingsUserId", table: "MusicalPreference");
            migrationBuilder.DropForeignKey(name: "FK_MusicalPreference_GeneralSettings_GeneralSettingsUserId", table: "MusicalPreference");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_Circle_CircleId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_ApplicationUser_MemberId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_PostTag_Blog_PostId", table: "PostTag");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Location_OrganizationAddressId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_ApplicationUser_PerformerId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_UserActivity_Activity_DoesCode", table: "UserActivity");
            migrationBuilder.DropForeignKey(name: "FK_UserActivity_PerformerProfile_UserId", table: "UserActivity");
            migrationBuilder.DropPrimaryKey(name: "PK_UserActivity", table: "UserActivity");
            migrationBuilder.DropPrimaryKey(name: "PK_MusicalPreference", table: "MusicalPreference");
            migrationBuilder.DropColumn(name: "DjSettingsUserId", table: "MusicalPreference");
            migrationBuilder.DropColumn(name: "GeneralSettingsUserId", table: "MusicalPreference");
            migrationBuilder.DropColumn(name: "TendencyId", table: "MusicalPreference");
            migrationBuilder.DropColumn(name: "SettingsClassName", table: "Activity");
            migrationBuilder.DropTable("MusicianSettings");
            migrationBuilder.DropTable("DjSettings");
            migrationBuilder.DropTable("GeneralSettings");
            migrationBuilder.DropTable("CoWorking");
            migrationBuilder.DropTable("FormationSettings");
            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "UserActivity",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:Serial", true);
            migrationBuilder.AddPrimaryKey(
                name: "PK_UserActivity",
                table: "UserActivity",
                column: "Id");
            migrationBuilder.AddColumn<long>(
                name: "OfferId",
                table: "PerformerProfile",
                nullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "OwnerProfileId",
                table: "MusicalPreference",
                nullable: true);
            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "MusicalPreference",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:Serial", true);
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "MusicalPreference",
                nullable: false,
                defaultValue: "");
            migrationBuilder.AddPrimaryKey(
                name: "PK_MusicalPreference",
                table: "MusicalPreference",
                column: "Id");
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
                name: "FK_CommandLine_Estimate_EstimateId",
                table: "CommandLine",
                column: "EstimateId",
                principalTable: "Estimate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_BookQuery_ApplicationUser_ClientId",
                table: "BookQuery",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_BookQuery_PerformerProfile_PerformerId",
                table: "BookQuery",
                column: "PerformerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_MusicalPreference_PerformerProfile_OwnerProfileId",
                table: "MusicalPreference",
                column: "OwnerProfileId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
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
                name: "FK_PerformerProfile_Service_OfferId",
                table: "PerformerProfile",
                column: "OfferId",
                principalTable: "Service",
                principalColumn: "Id",
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
