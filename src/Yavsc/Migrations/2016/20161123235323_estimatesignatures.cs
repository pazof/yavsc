
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace Yavsc.Migrations
{
    public partial class estimatesignatures : Migration
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
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_Circle_CircleId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_ApplicationUser_MemberId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_PostTag_Blog_PostId", table: "PostTag");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Activity_ActivityCode", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Location_OrganizationAddressId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_ApplicationUser_PerformerId", table: "PerformerProfile");
            migrationBuilder.DropColumn(name: "ClientApprouvalDate", table: "Estimate");
            migrationBuilder.DropColumn(name: "LatestValidationDate", table: "Estimate");
            migrationBuilder.AddColumn<byte[]>(
                name: "ClientSignature",
                table: "Estimate",
                nullable: true);
            migrationBuilder.AddColumn<DateTime>(
                name: "ClientValidationDate",
                table: "Estimate",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            migrationBuilder.AddColumn<byte[]>(
                name: "ProviderSignature",
                table: "Estimate",
                nullable: true);
            migrationBuilder.AddColumn<DateTime>(
                name: "ProviderValidationDate",
                table: "Estimate",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
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
                name: "FK_PerformerProfile_Activity_ActivityCode",
                table: "PerformerProfile",
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
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_ApplicationUser_ClientId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_PerformerProfile_PerformerId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_Circle_CircleId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_ApplicationUser_MemberId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_PostTag_Blog_PostId", table: "PostTag");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Activity_ActivityCode", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Location_OrganizationAddressId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_ApplicationUser_PerformerId", table: "PerformerProfile");
            migrationBuilder.DropColumn(name: "ClientSignature", table: "Estimate");
            migrationBuilder.DropColumn(name: "ClientValidationDate", table: "Estimate");
            migrationBuilder.DropColumn(name: "ProviderSignature", table: "Estimate");
            migrationBuilder.DropColumn(name: "ProviderValidationDate", table: "Estimate");
            migrationBuilder.AddColumn<DateTime>(
                name: "ClientApprouvalDate",
                table: "Estimate",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            migrationBuilder.AddColumn<DateTime>(
                name: "LatestValidationDate",
                table: "Estimate",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
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
                name: "FK_PerformerProfile_Activity_ActivityCode",
                table: "PerformerProfile",
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
        }
    }
}
