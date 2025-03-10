
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace Yavsc.Migrations
{
    public partial class BlogComment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_IdentityRoleClaim<string>_IdentityRole_RoleId", table: "AspNetRoleClaims");
            migrationBuilder.DropForeignKey(name: "FK_IdentityUserClaim<string>_ApplicationUser_UserId", table: "AspNetUserClaims");
            migrationBuilder.DropForeignKey(name: "FK_IdentityUserLogin<string>_ApplicationUser_UserId", table: "AspNetUserLogins");
            migrationBuilder.DropForeignKey(name: "FK_IdentityUserRole<string>_IdentityRole_RoleId", table: "AspNetUserRoles");
            migrationBuilder.DropForeignKey(name: "FK_IdentityUserRole<string>_ApplicationUser_UserId", table: "AspNetUserRoles");
            migrationBuilder.DropForeignKey(name: "FK_BlackListed_ApplicationUser_OwnerId", table: "BlackListed");
            migrationBuilder.DropForeignKey(name: "FK_CircleAuthorizationToBlogPost_BlogPost_BlogPostId", table: "CircleAuthorizationToBlogPost");
            migrationBuilder.DropForeignKey(name: "FK_CircleAuthorizationToBlogPost_Circle_CircleId", table: "CircleAuthorizationToBlogPost");
            migrationBuilder.DropForeignKey(name: "FK_AccountBalance_ApplicationUser_UserId", table: "AccountBalance");
            migrationBuilder.DropForeignKey(name: "FK_BalanceImpact_AccountBalance_BalanceId", table: "BalanceImpact");
            migrationBuilder.DropForeignKey(name: "FK_CommandLine_Estimate_EstimateId", table: "CommandLine");
            migrationBuilder.DropForeignKey(name: "FK_Estimate_ApplicationUser_ClientId", table: "Estimate");
            migrationBuilder.DropForeignKey(name: "FK_Estimate_PerformerProfile_OwnerId", table: "Estimate");
            migrationBuilder.DropForeignKey(name: "FK_BlogTag_BlogPost_PostId", table: "BlogTag");
            migrationBuilder.DropForeignKey(name: "FK_BlogTag_Tag_TagId", table: "BlogTag");
            migrationBuilder.DropForeignKey(name: "FK_Schedule_ApplicationUser_OwnerId", table: "Schedule");
            migrationBuilder.DropForeignKey(name: "FK_Connection_ApplicationUser_ApplicationUserId", table: "Connection");
            migrationBuilder.DropForeignKey(name: "FK_BrusherProfile_PerformerProfile_UserId", table: "BrusherProfile");
            migrationBuilder.DropForeignKey(name: "FK_HairCutQuery_Activity_ActivityCode", table: "HairCutQuery");
            migrationBuilder.DropForeignKey(name: "FK_HairCutQuery_ApplicationUser_ClientId", table: "HairCutQuery");
            migrationBuilder.DropForeignKey(name: "FK_HairCutQuery_PerformerProfile_PerformerId", table: "HairCutQuery");
            migrationBuilder.DropForeignKey(name: "FK_HairCutQuery_HairPrestation_PrestationId", table: "HairCutQuery");
            migrationBuilder.DropForeignKey(name: "FK_HairMultiCutQuery_Activity_ActivityCode", table: "HairMultiCutQuery");
            migrationBuilder.DropForeignKey(name: "FK_HairMultiCutQuery_ApplicationUser_ClientId", table: "HairMultiCutQuery");
            migrationBuilder.DropForeignKey(name: "FK_HairMultiCutQuery_PerformerProfile_PerformerId", table: "HairMultiCutQuery");
            migrationBuilder.DropForeignKey(name: "FK_HairPrestationCollectionItem_HairPrestation_PrestationId", table: "HairPrestationCollectionItem");
            migrationBuilder.DropForeignKey(name: "FK_HairPrestationCollectionItem_HairMultiCutQuery_QueryId", table: "HairPrestationCollectionItem");
            migrationBuilder.DropForeignKey(name: "FK_HairTaint_Color_ColorId", table: "HairTaint");
            migrationBuilder.DropForeignKey(name: "FK_HairTaintInstance_HairPrestation_PrestationId", table: "HairTaintInstance");
            migrationBuilder.DropForeignKey(name: "FK_HairTaintInstance_HairTaint_TaintId", table: "HairTaintInstance");
            migrationBuilder.DropForeignKey(name: "FK_DimissClicked_Notification_NotificationId", table: "DimissClicked");
            migrationBuilder.DropForeignKey(name: "FK_DimissClicked_ApplicationUser_UserId", table: "DimissClicked");
            migrationBuilder.DropForeignKey(name: "FK_Instrumentation_Instrument_InstrumentId", table: "Instrumentation");
            migrationBuilder.DropForeignKey(name: "FK_PayPalPayment_ApplicationUser_ExecutorId", table: "PayPalPayment");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_Circle_CircleId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_ApplicationUser_MemberId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_CommandForm_Activity_ActivityCode", table: "CommandForm");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Location_OrganizationAddressId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_ApplicationUser_PerformerId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_RdvQuery_Activity_ActivityCode", table: "RdvQuery");
            migrationBuilder.DropForeignKey(name: "FK_RdvQuery_ApplicationUser_ClientId", table: "RdvQuery");
            migrationBuilder.DropForeignKey(name: "FK_RdvQuery_PerformerProfile_PerformerId", table: "RdvQuery");
            migrationBuilder.DropForeignKey(name: "FK_UserActivity_Activity_DoesCode", table: "UserActivity");
            migrationBuilder.DropForeignKey(name: "FK_UserActivity_PerformerProfile_UserId", table: "UserActivity");
            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Content = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateModified = table.Column<DateTime>(nullable: false),
                    PostId = table.Column<long>(nullable: false),
                    UserCreated = table.Column<string>(nullable: true),
                    UserModified = table.Column<string>(nullable: true),
                    Visible = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comment_BlogPost_PostId",
                        column: x => x.PostId,
                        principalTable: "BlogPost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
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
                name: "FK_CircleAuthorizationToBlogPost_BlogPost_BlogPostId",
                table: "CircleAuthorizationToBlogPost",
                column: "BlogPostId",
                principalTable: "BlogPost",
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
                name: "FK_Estimate_PerformerProfile_OwnerId",
                table: "Estimate",
                column: "OwnerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_BlogTag_BlogPost_PostId",
                table: "BlogTag",
                column: "PostId",
                principalTable: "BlogPost",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_BlogTag_Tag_TagId",
                table: "BlogTag",
                column: "TagId",
                principalTable: "Tag",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_ApplicationUser_OwnerId",
                table: "Schedule",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_Connection_ApplicationUser_ApplicationUserId",
                table: "Connection",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_BrusherProfile_PerformerProfile_UserId",
                table: "BrusherProfile",
                column: "UserId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQuery_Activity_ActivityCode",
                table: "HairCutQuery",
                column: "ActivityCode",
                principalTable: "Activity",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQuery_ApplicationUser_ClientId",
                table: "HairCutQuery",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQuery_PerformerProfile_PerformerId",
                table: "HairCutQuery",
                column: "PerformerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQuery_HairPrestation_PrestationId",
                table: "HairCutQuery",
                column: "PrestationId",
                principalTable: "HairPrestation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQuery_Activity_ActivityCode",
                table: "HairMultiCutQuery",
                column: "ActivityCode",
                principalTable: "Activity",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQuery_ApplicationUser_ClientId",
                table: "HairMultiCutQuery",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQuery_PerformerProfile_PerformerId",
                table: "HairMultiCutQuery",
                column: "PerformerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_HairPrestationCollectionItem_HairPrestation_PrestationId",
                table: "HairPrestationCollectionItem",
                column: "PrestationId",
                principalTable: "HairPrestation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_HairPrestationCollectionItem_HairMultiCutQuery_QueryId",
                table: "HairPrestationCollectionItem",
                column: "QueryId",
                principalTable: "HairMultiCutQuery",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_HairTaint_Color_ColorId",
                table: "HairTaint",
                column: "ColorId",
                principalTable: "Color",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_HairTaintInstance_HairPrestation_PrestationId",
                table: "HairTaintInstance",
                column: "PrestationId",
                principalTable: "HairPrestation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_HairTaintInstance_HairTaint_TaintId",
                table: "HairTaintInstance",
                column: "TaintId",
                principalTable: "HairTaint",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
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
                name: "FK_PayPalPayment_ApplicationUser_ExecutorId",
                table: "PayPalPayment",
                column: "ExecutorId",
                principalTable: "AspNetUsers",
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
                name: "FK_RdvQuery_Activity_ActivityCode",
                table: "RdvQuery",
                column: "ActivityCode",
                principalTable: "Activity",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_RdvQuery_ApplicationUser_ClientId",
                table: "RdvQuery",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_RdvQuery_PerformerProfile_PerformerId",
                table: "RdvQuery",
                column: "PerformerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
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
            migrationBuilder.DropForeignKey(name: "FK_CircleAuthorizationToBlogPost_BlogPost_BlogPostId", table: "CircleAuthorizationToBlogPost");
            migrationBuilder.DropForeignKey(name: "FK_CircleAuthorizationToBlogPost_Circle_CircleId", table: "CircleAuthorizationToBlogPost");
            migrationBuilder.DropForeignKey(name: "FK_AccountBalance_ApplicationUser_UserId", table: "AccountBalance");
            migrationBuilder.DropForeignKey(name: "FK_BalanceImpact_AccountBalance_BalanceId", table: "BalanceImpact");
            migrationBuilder.DropForeignKey(name: "FK_CommandLine_Estimate_EstimateId", table: "CommandLine");
            migrationBuilder.DropForeignKey(name: "FK_Estimate_ApplicationUser_ClientId", table: "Estimate");
            migrationBuilder.DropForeignKey(name: "FK_Estimate_PerformerProfile_OwnerId", table: "Estimate");
            migrationBuilder.DropForeignKey(name: "FK_BlogTag_BlogPost_PostId", table: "BlogTag");
            migrationBuilder.DropForeignKey(name: "FK_BlogTag_Tag_TagId", table: "BlogTag");
            migrationBuilder.DropForeignKey(name: "FK_Schedule_ApplicationUser_OwnerId", table: "Schedule");
            migrationBuilder.DropForeignKey(name: "FK_Connection_ApplicationUser_ApplicationUserId", table: "Connection");
            migrationBuilder.DropForeignKey(name: "FK_BrusherProfile_PerformerProfile_UserId", table: "BrusherProfile");
            migrationBuilder.DropForeignKey(name: "FK_HairCutQuery_Activity_ActivityCode", table: "HairCutQuery");
            migrationBuilder.DropForeignKey(name: "FK_HairCutQuery_ApplicationUser_ClientId", table: "HairCutQuery");
            migrationBuilder.DropForeignKey(name: "FK_HairCutQuery_PerformerProfile_PerformerId", table: "HairCutQuery");
            migrationBuilder.DropForeignKey(name: "FK_HairCutQuery_HairPrestation_PrestationId", table: "HairCutQuery");
            migrationBuilder.DropForeignKey(name: "FK_HairMultiCutQuery_Activity_ActivityCode", table: "HairMultiCutQuery");
            migrationBuilder.DropForeignKey(name: "FK_HairMultiCutQuery_ApplicationUser_ClientId", table: "HairMultiCutQuery");
            migrationBuilder.DropForeignKey(name: "FK_HairMultiCutQuery_PerformerProfile_PerformerId", table: "HairMultiCutQuery");
            migrationBuilder.DropForeignKey(name: "FK_HairPrestationCollectionItem_HairPrestation_PrestationId", table: "HairPrestationCollectionItem");
            migrationBuilder.DropForeignKey(name: "FK_HairPrestationCollectionItem_HairMultiCutQuery_QueryId", table: "HairPrestationCollectionItem");
            migrationBuilder.DropForeignKey(name: "FK_HairTaint_Color_ColorId", table: "HairTaint");
            migrationBuilder.DropForeignKey(name: "FK_HairTaintInstance_HairPrestation_PrestationId", table: "HairTaintInstance");
            migrationBuilder.DropForeignKey(name: "FK_HairTaintInstance_HairTaint_TaintId", table: "HairTaintInstance");
            migrationBuilder.DropForeignKey(name: "FK_DimissClicked_Notification_NotificationId", table: "DimissClicked");
            migrationBuilder.DropForeignKey(name: "FK_DimissClicked_ApplicationUser_UserId", table: "DimissClicked");
            migrationBuilder.DropForeignKey(name: "FK_Instrumentation_Instrument_InstrumentId", table: "Instrumentation");
            migrationBuilder.DropForeignKey(name: "FK_PayPalPayment_ApplicationUser_ExecutorId", table: "PayPalPayment");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_Circle_CircleId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_CircleMember_ApplicationUser_MemberId", table: "CircleMember");
            migrationBuilder.DropForeignKey(name: "FK_CommandForm_Activity_ActivityCode", table: "CommandForm");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_Location_OrganizationAddressId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_PerformerProfile_ApplicationUser_PerformerId", table: "PerformerProfile");
            migrationBuilder.DropForeignKey(name: "FK_RdvQuery_Activity_ActivityCode", table: "RdvQuery");
            migrationBuilder.DropForeignKey(name: "FK_RdvQuery_ApplicationUser_ClientId", table: "RdvQuery");
            migrationBuilder.DropForeignKey(name: "FK_RdvQuery_PerformerProfile_PerformerId", table: "RdvQuery");
            migrationBuilder.DropForeignKey(name: "FK_UserActivity_Activity_DoesCode", table: "UserActivity");
            migrationBuilder.DropForeignKey(name: "FK_UserActivity_PerformerProfile_UserId", table: "UserActivity");
            migrationBuilder.DropTable("Comment");
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
                name: "FK_CircleAuthorizationToBlogPost_BlogPost_BlogPostId",
                table: "CircleAuthorizationToBlogPost",
                column: "BlogPostId",
                principalTable: "BlogPost",
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
                name: "FK_Estimate_PerformerProfile_OwnerId",
                table: "Estimate",
                column: "OwnerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_BlogTag_BlogPost_PostId",
                table: "BlogTag",
                column: "PostId",
                principalTable: "BlogPost",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_BlogTag_Tag_TagId",
                table: "BlogTag",
                column: "TagId",
                principalTable: "Tag",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_ApplicationUser_OwnerId",
                table: "Schedule",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Connection_ApplicationUser_ApplicationUserId",
                table: "Connection",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_BrusherProfile_PerformerProfile_UserId",
                table: "BrusherProfile",
                column: "UserId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQuery_Activity_ActivityCode",
                table: "HairCutQuery",
                column: "ActivityCode",
                principalTable: "Activity",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQuery_ApplicationUser_ClientId",
                table: "HairCutQuery",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQuery_PerformerProfile_PerformerId",
                table: "HairCutQuery",
                column: "PerformerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQuery_HairPrestation_PrestationId",
                table: "HairCutQuery",
                column: "PrestationId",
                principalTable: "HairPrestation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQuery_Activity_ActivityCode",
                table: "HairMultiCutQuery",
                column: "ActivityCode",
                principalTable: "Activity",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQuery_ApplicationUser_ClientId",
                table: "HairMultiCutQuery",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQuery_PerformerProfile_PerformerId",
                table: "HairMultiCutQuery",
                column: "PerformerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_HairPrestationCollectionItem_HairPrestation_PrestationId",
                table: "HairPrestationCollectionItem",
                column: "PrestationId",
                principalTable: "HairPrestation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_HairPrestationCollectionItem_HairMultiCutQuery_QueryId",
                table: "HairPrestationCollectionItem",
                column: "QueryId",
                principalTable: "HairMultiCutQuery",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_HairTaint_Color_ColorId",
                table: "HairTaint",
                column: "ColorId",
                principalTable: "Color",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_HairTaintInstance_HairPrestation_PrestationId",
                table: "HairTaintInstance",
                column: "PrestationId",
                principalTable: "HairPrestation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_HairTaintInstance_HairTaint_TaintId",
                table: "HairTaintInstance",
                column: "TaintId",
                principalTable: "HairTaint",
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
                name: "FK_PayPalPayment_ApplicationUser_ExecutorId",
                table: "PayPalPayment",
                column: "ExecutorId",
                principalTable: "AspNetUsers",
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
                name: "FK_RdvQuery_Activity_ActivityCode",
                table: "RdvQuery",
                column: "ActivityCode",
                principalTable: "Activity",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_RdvQuery_ApplicationUser_ClientId",
                table: "RdvQuery",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_RdvQuery_PerformerProfile_PerformerId",
                table: "RdvQuery",
                column: "PerformerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId",
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
