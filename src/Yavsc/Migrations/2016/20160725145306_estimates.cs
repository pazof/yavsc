
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Yavsc.Migrations
{
    public partial class estimates : Migration
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
            migrationBuilder.DropForeignKey(name: "FK_CommandLine_RDVEstimate_RDVEstimateId", table: "CommandLine");
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_ApplicationUser_ClientId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_PerformerProfile_PerformerId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_Circle_CircleId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_ApplicationUser_MemberId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_Contact_ApplicationUser_OwnerId", table: "Contact");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Activity_ActivityCode", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Location_OrganisationAddressId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_ApplicationUser_PerfomerId", table: "PerformerProfile");
            migrationBuilder.DropColumn(name: "RDVEstimateId", table: "CommandLine");
            migrationBuilder.DropTable("RDVEstimate");
            migrationBuilder.CreateTable(
                name: "Estimate",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    AttachedFilesString = table.Column<string>(nullable: true),
                    AttachedGraphicsString = table.Column<string>(nullable: true),
                    CommandId = table.Column<long>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: true),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estimate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Estimate_BookQuery_CommandId",
                        column: x => x.CommandId,
                        principalTable: "BookQuery",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.AlterColumn<DateTime>(
                name: "posted",
                table: "Blog",
                nullable: false,
                defaultValueSql: "LOCALTIMESTAMP");
            migrationBuilder.AlterColumn<string>(
                name: "AuthorId",
                table: "Blog",
                nullable: true);
            migrationBuilder.AddColumn<long>(
                name: "EstimateId",
                table: "CommandLine",
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
                onDelete: ReferentialAction.Restrict);
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
            migrationBuilder.DropForeignKey(name: "FK_CommandLine_Estimate_EstimateId", table: "CommandLine");
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_ApplicationUser_ClientId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_BookQuery_PerformerProfile_PerformerId", table: "BookQuery");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_Circle_CircleId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_ApplicationUser_MemberId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_Contact_ApplicationUser_OwnerId", table: "Contact");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Activity_ActivityCode", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Location_OrganisationAddressId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_ApplicationUser_PerfomerId", table: "PerformerProfile");
            migrationBuilder.DropColumn(name: "EstimateId", table: "CommandLine");
            migrationBuilder.DropTable("Estimate");
            migrationBuilder.CreateTable(
                name: "RDVEstimate",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    AttachedFilesString = table.Column<string>(nullable: true),
                    AttachedGraphicsString = table.Column<string>(nullable: true),
                    CommandId = table.Column<long>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: true),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RDVEstimate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RDVEstimate_BookQuery_CommandId",
                        column: x => x.CommandId,
                        principalTable: "BookQuery",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.AlterColumn<DateTime>(
                name: "posted",
                table: "Blog",
                nullable: false);
            migrationBuilder.AlterColumn<string>(
                name: "AuthorId",
                table: "Blog",
                nullable: false);
            migrationBuilder.AddColumn<long>(
                name: "RDVEstimateId",
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
                name: "FK_CommandLine_RDVEstimate_RDVEstimateId",
                table: "CommandLine",
                column: "RDVEstimateId",
                principalTable: "RDVEstimate",
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
