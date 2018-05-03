using Microsoft.Data.Entity.Migrations;

namespace Yavsc.Migrations
{
    public partial class tokenExpiresIn : Migration
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
            migrationBuilder.DropForeignKey(name: "FK_Blog_ApplicationUser_AuthorId", table: "Blog");
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_ApplicationUser_ClientId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_Location_LocationId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_PerformerProfile_PerformerId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_Circle_CircleId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_ApplicationUser_MemberId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_Command_ApplicationUser_ClientId", table: "Command");
            migrationBuilder.DropForeignKey(name: "FK_Command_PerformerProfile_PerformerId", table: "Command");
            migrationBuilder.DropForeignKey(name: "FK_Contact_ApplicationUser_OwnerId", table: "Contact");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Activity_ActivityCode", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Location_OrganisationAddressId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_ApplicationUser_PerfomerId", table: "PerformerProfile");
            migrationBuilder.CreateTable(
                name: "Application",
                columns: table => new
                {
                    ApplicationID = table.Column<string>(nullable: false),
                    DisplayName = table.Column<string>(nullable: true),
                    LogoutRedirectUri = table.Column<string>(nullable: true),
                    RedirectUri = table.Column<string>(nullable: true),
                    Secret = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Application", x => x.ApplicationID);
                });
            migrationBuilder.AddColumn<bool>(
                name: "Public",
                table: "Service",
                nullable: false,
                defaultValue: false);
            migrationBuilder.AddColumn<string>(
                name: "ExpiresIn",
                table: "OAuth2Tokens",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "Public",
                table: "BaseProduct",
                nullable: false,
                defaultValue: false);
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
                name: "FK_Blog_ApplicationUser_AuthorId",
                table: "Blog",
                column: "AuthorId",
                principalTable: "AspNetUsers",
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
                name: "FK_BookQuery_Location_LocationId",
                table: "BookQuery",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_BookQuery_PerformerProfile_PerformerId",
                table: "BookQuery",
                column: "PerformerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerfomerId",
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
                name: "FK_Command_ApplicationUser_ClientId",
                table: "Command",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_Command_PerformerProfile_PerformerId",
                table: "Command",
                column: "PerformerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerfomerId",
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
                name: "FK_PerformerProfile_Location_OrganisationAddressId",
                table: "PerformerProfile",
                column: "OrganisationAddressId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_PerformerProfile_ApplicationUser_PerfomerId",
                table: "PerformerProfile",
                column: "PerfomerId",
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
            migrationBuilder.DropForeignKey(name: "FK_Blog_ApplicationUser_AuthorId", table: "Blog");
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_ApplicationUser_ClientId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_Location_LocationId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_PerformerProfile_PerformerId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_Circle_CircleId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_ApplicationUser_MemberId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_Command_ApplicationUser_ClientId", table: "Command");
            migrationBuilder.DropForeignKey(name: "FK_Command_PerformerProfile_PerformerId", table: "Command");
            migrationBuilder.DropForeignKey(name: "FK_Contact_ApplicationUser_OwnerId", table: "Contact");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Activity_ActivityCode", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Location_OrganisationAddressId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_ApplicationUser_PerfomerId", table: "PerformerProfile");
            migrationBuilder.DropColumn(name: "Public", table: "Service");
            migrationBuilder.DropColumn(name: "ExpiresIn", table: "OAuth2Tokens");
            migrationBuilder.DropColumn(name: "Public", table: "BaseProduct");
            migrationBuilder.DropTable("Application");
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
                name: "FK_Blog_ApplicationUser_AuthorId",
                table: "Blog",
                column: "AuthorId",
                principalTable: "AspNetUsers",
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
                name: "FK_BookQuery_Location_LocationId",
                table: "BookQuery",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_BookQuery_PerformerProfile_PerformerId",
                table: "BookQuery",
                column: "PerformerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerfomerId",
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
                name: "FK_Command_ApplicationUser_ClientId",
                table: "Command",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Command_PerformerProfile_PerformerId",
                table: "Command",
                column: "PerformerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerfomerId",
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
                name: "FK_PerformerProfile_Location_OrganisationAddressId",
                table: "PerformerProfile",
                column: "OrganisationAddressId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_PerformerProfile_ApplicationUser_PerfomerId",
                table: "PerformerProfile",
                column: "PerfomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
