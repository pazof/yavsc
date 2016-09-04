using Microsoft.Data.Entity.Migrations;

namespace Yavsc.Migrations
{
    public partial class products : Migration
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
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_ApplicationUser_ClientId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_PerformerProfile_PerformerId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_Circle_CircleId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_ApplicationUser_MemberId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_Contact_ApplicationUser_OwnerId", table: "Contact");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Activity_ActivityCode", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Location_OrganizationAddressId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_ApplicationUser_PerformerId", table: "PerformerProfile");
            migrationBuilder.DropColumn(name: "Billing", table: "Service");
            migrationBuilder.DropColumn(name: "Pricing", table: "Service");
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "BaseProduct",
                nullable: false,
                defaultValue: "");
            migrationBuilder.AddColumn<decimal>(
                name: "Depth",
                table: "BaseProduct",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "Height",
                table: "BaseProduct",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "BaseProduct",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "BaseProduct",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "Width",
                table: "BaseProduct",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "Estimate",
                nullable: false,
                defaultValue: "");
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Estimate",
                nullable: false,
                defaultValue: "");
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
                name: "FK_Contact_ApplicationUser_OwnerId",
                table: "Contact",
                column: "OwnerId",
                principalTable: "AspNetUsers",
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
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_ApplicationUser_ClientId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_PerformerProfile_PerformerId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_Circle_CircleId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_ApplicationUser_MemberId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_Contact_ApplicationUser_OwnerId", table: "Contact");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Activity_ActivityCode", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Location_OrganizationAddressId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_ApplicationUser_PerformerId", table: "PerformerProfile");
            migrationBuilder.DropColumn(name: "Discriminator", table: "BaseProduct");
            migrationBuilder.DropColumn(name: "Depth", table: "BaseProduct");
            migrationBuilder.DropColumn(name: "Height", table: "BaseProduct");
            migrationBuilder.DropColumn(name: "Price", table: "BaseProduct");
            migrationBuilder.DropColumn(name: "Weight", table: "BaseProduct");
            migrationBuilder.DropColumn(name: "Width", table: "BaseProduct");
            migrationBuilder.DropColumn(name: "ClientId", table: "Estimate");
            migrationBuilder.DropColumn(name: "OwnerId", table: "Estimate");
            migrationBuilder.AddColumn<int>(
                name: "Billing",
                table: "Service",
                nullable: true);
            migrationBuilder.AddColumn<decimal>(
                name: "Pricing",
                table: "Service",
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
                name: "FK_Contact_ApplicationUser_OwnerId",
                table: "Contact",
                column: "OwnerId",
                principalTable: "AspNetUsers",
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
