using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class net7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from \"Announce\" where \"OwnerId\" IS NULL");
            
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_BlogPost_PostId",
                table: "Comment"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_CircleAuthorizationToBlogPost_BlogPost_BlogPostId",
                table: "CircleAuthorizationToBlogPost");

            migrationBuilder.DropForeignKey(
                name: "FK_CircleAuthorizationToBlogPost_Circle_CircleId",
                table: "CircleAuthorizationToBlogPost");


              migrationBuilder.DropForeignKey(
                name: "FK_BlogTag_BlogPost_PostId",
                table: "BlogTag");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogTag_Tag_TagId",
                table: "BlogTag");

            
           
            
            migrationBuilder.DropForeignKey(
                name: "FK_Activity_Activity_ParentCode",
                table: "Activity");
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Location_PostalAddressId",
                table: "AspNetUsers");
           /*  migrationBuilder.DropPrimaryKey(
                name: "PK_Blog",
                table: "BlogPost");
              
                migrationBuilder.DropForeignKey(
                name: "FK_AccountBalance_AspNetUsers_UserId",
                table: "AccountBalance");   */

            migrationBuilder.DropForeignKey(
                name: "FK_Announce_AspNetUsers_OwnerId",
                table: "Announce");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims"); 

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_BankIdentity_BankInfoId",
                table: "AspNetUsers");

           
            migrationBuilder.DropForeignKey(
                name: "FK_Ban_AspNetUsers_TargetId",
                table: "Ban");
            migrationBuilder.DropForeignKey(
                name: "FK_BalanceImpact_AccountBalance_BalanceId",
                table: "BalanceImpact");

            migrationBuilder.DropForeignKey(
                name: "FK_BlackListed_AspNetUsers_OwnerId",
                table: "BlackListed"); 
            migrationBuilder.DropForeignKey(
                name: "FK_BlackListed_AspNetUsers_UserId",
                table: "BlackListed");
            migrationBuilder.DropForeignKey(
                name: "FK_BlogPost_AspNetUsers_AuthorId",
                table: "BlogPost");

        /*    migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUser_BankIdentity_BankInfoId",
                table: "AspNetUsers"); 
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUser_Location_PostalAddressId",
                table: "AspNetUsers");
            migrationBuilder.DropForeignKey(
                name: "FK_Ban_ApplicationUser_TargetId",
                table: "Ban");

*/

            migrationBuilder.DropForeignKey(
                name: "FK_BlogTrad_AspNetUsers_TraducerId",
                table: "BlogTrad");

            migrationBuilder.DropForeignKey(
                name: "FK_BrusherProfile_PerformerProfile_UserId",
                table: "BrusherProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_BrusherProfile_Schedule_ScheduleOwnerId",
                table: "BrusherProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_Bug_Feature_FeatureId",
                table: "Bug"); 
            migrationBuilder.DropForeignKey(
                name: "FK_ChatConnection_AspNetUsers_ApplicationUserId",
                table: "ChatConnection");
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoom_AspNetUsers_OwnerId",
                table: "ChatRoom");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoomAccess_AspNetUsers_UserId",
                table: "ChatRoomAccess");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoomAccess_ChatRoom_ChannelName",
                table: "ChatRoomAccess");

            migrationBuilder.DropForeignKey(
                name: "FK_CircleMember_AspNetUsers_MemberId",
                table: "CircleMember"); 

            migrationBuilder.DropForeignKey(
                name: "FK_CircleMember_Circle_CircleId",
                table: "CircleMember");

            migrationBuilder.DropForeignKey(
                name: "FK_CommandForm_Activity_ActivityCode",
                table: "CommandForm");

            migrationBuilder.DropForeignKey(
                name: "FK_CommandLine_EstimateTemplate_EstimateTemplateId",
                table: "CommandLine");

            migrationBuilder.DropForeignKey(
                name: "FK_CommandLine_Estimate_EstimateId",
                table: "CommandLine");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_AspNetUsers_AuthorId",
                table: "Comment");


            migrationBuilder.DropForeignKey(
                name: "FK_Contact_PostalAddress_AddressId",
                table: "Contact");

            migrationBuilder.DropForeignKey(
                name: "FK_CoWorking_AspNetUsers_WorkingForId",
                table: "CoWorking");

            migrationBuilder.DropForeignKey(
                name: "FK_CoWorking_PerformerProfile_PerformerId",
                table: "CoWorking");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceDeclaration_AspNetUsers_DeviceOwnerId",
                table: "DeviceDeclaration"); 

            migrationBuilder.DropForeignKey(
                name: "FK_DimissClicked_AspNetUsers_UserId",
                table: "DimissClicked");

            migrationBuilder.DropForeignKey(
                name: "FK_DimissClicked_Notification_NotificationId",
                table: "DimissClicked");

            migrationBuilder.DropForeignKey(
                name: "FK_Estimate_AspNetUsers_ClientId",
                table: "Estimate");

            migrationBuilder.DropForeignKey(
                name: "FK_Estimate_PerformerProfile_OwnerId",
                table: "Estimate");

            migrationBuilder.DropForeignKey(
                name: "FK_Estimate_RdvQuery_CommandId",
                table: "Estimate");

           migrationBuilder.DropForeignKey(
                name: "FK_GitRepositoryReference_AspNetUsers_OwnerId",
                table: "GitRepositoryReference"); 

            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQuery_Activity_ActivityCode",
                table: "HairCutQuery");

            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQuery_AspNetUsers_ClientId",
                table: "HairCutQuery"); 

            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQuery_BrusherProfile_SelectedProfileUserId",
                table: "HairCutQuery");

            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQuery_HairPrestation_PrestationId",
                table: "HairCutQuery");

            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQuery_Location_LocationId",
                table: "HairCutQuery");


            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQuery_PayPalPayment_PaymentId",
                table: "HairCutQuery"); 

            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQuery_PerformerProfile_PerformerId",
                table: "HairCutQuery");

            migrationBuilder.DropForeignKey(
                name: "FK_HairMultiCutQuery_Activity_ActivityCode",
                table: "HairMultiCutQuery");

             migrationBuilder.DropForeignKey(
                name: "FK_HairMultiCutQuery_AspNetUsers_ClientId",
                table: "HairMultiCutQuery"); 

            migrationBuilder.DropForeignKey(
                name: "FK_HairMultiCutQuery_Location_LocationId",
                table: "HairMultiCutQuery");

            migrationBuilder.DropForeignKey(
                name: "FK_HairMultiCutQuery_PayPalPayment_PaymentId",
                table: "HairMultiCutQuery");

            migrationBuilder.DropForeignKey(
                name: "FK_HairMultiCutQuery_PerformerProfile_PerformerId",
                table: "HairMultiCutQuery");

            migrationBuilder.DropForeignKey(
                name: "FK_HairPrestationCollectionItem_HairMultiCutQuery_QueryId",
                table: "HairPrestationCollectionItem");

            migrationBuilder.DropForeignKey(
                name: "FK_HairPrestationCollectionItem_HairPrestation_PrestationId",
                table: "HairPrestationCollectionItem");

            migrationBuilder.DropForeignKey(
                name: "FK_HairTaint_Color_ColorId",
                table: "HairTaint");

            migrationBuilder.DropForeignKey(
                name: "FK_HairTaintInstance_HairPrestation_PrestationId",
                table: "HairTaintInstance");

            migrationBuilder.DropForeignKey(
                name: "FK_HairTaintInstance_HairTaint_TaintId",
                table: "HairTaintInstance");

            migrationBuilder.DropForeignKey(
                name: "FK_Instrumentation_Instrument_InstrumentId",
                table: "Instrumentation");

            migrationBuilder.DropForeignKey(
                name: "FK_Instrumentation_PerformerProfile_UserId",
                table: "Instrumentation");

            migrationBuilder.DropForeignKey(
                name: "FK_InstrumentRating_Instrument_InstrumentId",
                table: "InstrumentRating");

            migrationBuilder.DropForeignKey(
                name: "FK_InstrumentRating_PerformerProfile_OwnerId",
                table: "InstrumentRating");

            migrationBuilder.DropForeignKey(
                name: "FK_LiveFlow_AspNetUsers_OwnerId",
                table: "LiveFlow"); 

            migrationBuilder.DropForeignKey(
                name: "FK_PayPalPayment_AspNetUsers_ExecutorId",
                table: "PayPalPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_PerformerProfile_AspNetUsers_PerformerId",
                table: "PerformerProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_PerformerProfile_Location_OrganizationAddressId",
                table: "PerformerProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Activity_ActivityCode",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_AspNetUsers_ClientId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_GitRepositoryReference_GitId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_PayPalPayment_PaymentId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_PerformerProfile_PerformerId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectBuildConfiguration_Project_ProjectId",
                table: "ProjectBuildConfiguration");

            migrationBuilder.DropForeignKey(
                name: "FK_RdvQuery_Activity_ActivityCode",
                table: "RdvQuery");

            migrationBuilder.DropForeignKey(
                name: "FK_RdvQuery_AspNetUsers_ClientId",
                table: "RdvQuery"); 

            migrationBuilder.DropForeignKey(
                name: "FK_RdvQuery_Location_LocationId",
                table: "RdvQuery");

            migrationBuilder.DropForeignKey(
                name: "FK_RdvQuery_PayPalPayment_PaymentId",
                table: "RdvQuery"); 

            migrationBuilder.DropForeignKey(
                name: "FK_RdvQuery_PerformerProfile_PerformerId",
                table: "RdvQuery");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_AspNetUsers_OwnerId",
                table: "Schedule");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledEvent_Period_PeriodStart_PeriodEnd",
                table: "ScheduledEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_Service_Activity_ContextId",
                table: "Service");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActivity_Activity_DoesCode",
                table: "UserActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActivity_PerformerProfile_UserId",
                table: "UserActivity");


            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserActivity",
                table: "UserActivity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tag",
                table: "Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Skill",
                table: "Skill");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Service",
                table: "Service");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshToken",
                table: "RefreshToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RdvQuery",
                table: "RdvQuery");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Product",
                table: "Product");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PerformerProfile",
                table: "PerformerProfile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Location",
                table: "Location");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HairMultiCutQuery",
                table: "HairMultiCutQuery");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HairCutQuery",
                table: "HairCutQuery");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExceptionSIREN",
                table: "ExceptionSIREN");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EstimateTemplate",
                table: "EstimateTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Estimate",
                table: "Estimate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Client",
                table: "Client");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CircleMember",
                table: "CircleMember");

            /* migrationBuilder.DropPrimaryKey(
                name: "PK_BlogPost",
                table: "BlogPost"); */

            migrationBuilder.DropPrimaryKey(
                name: "PK_BalanceImpact",
                table: "BalanceImpact");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Activity",
                table: "Activity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccountBalance",
                table: "AccountBalance");

            migrationBuilder.RenameTable(
                name: "UserActivity",
                newName: "UserActivities");

            migrationBuilder.RenameTable(
                name: "Tag",
                newName: "Tags");

            migrationBuilder.RenameTable(
                name: "Skill",
                newName: "SiteSkills");

            migrationBuilder.RenameTable(
                name: "Service",
                newName: "Services");

            migrationBuilder.RenameTable(
                name: "RefreshToken",
                newName: "RefreshTokens");

            migrationBuilder.RenameTable(
                name: "RdvQuery",
                newName: "RdvQueries");

            migrationBuilder.RenameTable(
                name: "Product",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "PerformerProfile",
                newName: "Performers");

            migrationBuilder.RenameTable(
                name: "Location",
                newName: "Locations");

            migrationBuilder.RenameTable(
                name: "HairMultiCutQuery",
                newName: "HairMultiCutQueries");

            migrationBuilder.RenameTable(
                name: "HairCutQuery",
                newName: "HairCutQueries");

            migrationBuilder.RenameTable(
                name: "ExceptionSIREN",
                newName: "ExceptionsSIREN");

            migrationBuilder.RenameTable(
                name: "EstimateTemplate",
                newName: "EstimateTemplates");

            migrationBuilder.RenameTable(
                name: "Estimate",
                newName: "Estimates");

            migrationBuilder.RenameTable(
                name: "Client",
                newName: "Applications");

            migrationBuilder.RenameTable(
                name: "CircleMember",
                newName: "CircleMembers");

            migrationBuilder.RenameTable(
                name: "BlogPost",
                newName: "Blogspot");

            migrationBuilder.RenameTable(
                name: "BalanceImpact",
                newName: "BankBook");

            migrationBuilder.RenameTable(
                name: "Activity",
                newName: "Activities");

            migrationBuilder.RenameTable(
                name: "AccountBalance",
                newName: "BankStatus");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PeriodStart",
                table: "ScheduledEvent",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PeriodEnd",
                table: "ScheduledEvent",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "ScheduledEvent",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProjectBuildConfiguration",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "ProjectBuildConfiguration",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "Project",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "Project",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "Project",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "Project",
                type: "text",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Project",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Project",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Project",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Project",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Street2",
                table: "PostalAddress",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Street1",
                table: "PostalAddress",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "PostalAddress",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Province",
                table: "PostalAddress",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "PostalAddress",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "PostalAddress",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "PostalAddress",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "PostalAddress",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "PayPalPayment",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "PayPalPayment",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "PayPalPayment",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaypalPayerId",
                table: "PayPalPayment",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrderReference",
                table: "PayPalPayment",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExecutorId",
                table: "PayPalPayment",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "Option",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "Option",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Option",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TokenType",
                table: "OAuth2Tokens",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "OAuth2Tokens",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExpiresIn",
                table: "OAuth2Tokens",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "OAuth2Tokens",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "Notification",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "tag",
                table: "Notification",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "sound",
                table: "Notification",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "icon",
                table: "Notification",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "exclam",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "exclam");

            migrationBuilder.AlterColumn<string>(
                name: "color",
                table: "Notification",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "click_action",
                table: "Notification",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "body",
                table: "Notification",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Target",
                table: "Notification",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Notification",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "MusicalTendency",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "MusicalTendency",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "MailingTemplate",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "MailingTemplate",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "MailingTemplate",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReplyToAddress",
                table: "MailingTemplate",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Body",
                table: "MailingTemplate",
                type: "character varying(65536)",
                maxLength: 65536,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(65536)",
                oldMaxLength: 65536,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "LiveFlow",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Pitch",
                table: "LiveFlow",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "LiveFlow",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MediaType",
                table: "LiveFlow",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DifferedFileName",
                table: "LiveFlow",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "LiveFlow",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "InstrumentRating",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Instrument",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Instrument",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Rel",
                table: "HyperLink",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "HyperLink",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "HairTaint",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "HairTaint",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "HairPrestationCollectionItem",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "HairPrestation",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "GitRepositoryReference",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "GitRepositoryReference",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "GitRepositoryReference",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Branch",
                table: "GitRepositoryReference",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "GitRepositoryReference",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "FormationSettings",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "Form",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShortName",
                table: "Feature",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Feature",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Feature",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "SoundCloudId",
                table: "DjSettings",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "DeviceDeclaration",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Platform",
                table: "DeviceDeclaration",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "DeviceDeclaration",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceOwnerId",
                table: "DeviceDeclaration",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeclarationDate",
                table: "DeviceDeclaration",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "LOCALTIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "WorkingForId",
                table: "CoWorking",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PerformerId",
                table: "CoWorking",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "CoWorking",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Contact",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EMail",
                table: "Contact",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "Comment",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "Comment",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Comment",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Comment",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "CommandLine",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "CommandLine",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "CommandForm",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActivityCode",
                table: "CommandForm",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActionName",
                table: "CommandForm",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "CommandForm",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Color",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Color",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "ClientProviderInfo",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "ClientProviderInfo",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EMail",
                table: "ClientProviderInfo",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Avatar",
                table: "ClientProviderInfo",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Circle",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Circle",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Circle",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "ChatRoomAccess",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "ChatRoom",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "ChatRoom",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "ChatRoom",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "ChatRoom",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "ChatConnection",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Bug",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "FeatureId",
                table: "Bug",
                type: "bigint",
                nullable: true,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Bug",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Bug",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "ScheduleOwnerId",
                table: "BrusherProfile",
                type: "text",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TraducerId",
                table: "BlogTrad",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "BlogTrad",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Body",
                table: "BlogTrad",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "BlackListed",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "WicketCode",
                table: "BankIdentity",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IBAN",
                table: "BankIdentity",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BankCode",
                table: "BankIdentity",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BIC",
                table: "BankIdentity",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "BankIdentity",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "BankIdentity",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "Ban",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "Ban",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Ban",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "PostalAddressId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DedicatedGoogleCalendar",
                table: "AspNetUsers",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "BankInfoId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Avatar",
                table: "AspNetUsers",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "/images/Users/icon_user.png",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true,
                oldDefaultValue: "/images/Users/icon_user.png");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "AspNetUserClaims",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "AspNetRoleClaims",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "Announce",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Sender",
                table: "Announce",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Announce",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Announce",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Announce",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tags",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Tags",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SiteSkills",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "SiteSkills",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Services",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Services",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContextId",
                table: "Services",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Services",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "RdvQueries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "RdvQueries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "RdvQueries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "RdvQueries",
                type: "text",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "LocationId",
                table: "RdvQueries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "RdvQueries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "RdvQueries",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Products",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "WebSite",
                table: "Performers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SIREN",
                table: "Performers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Locations",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Locations",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "HairMultiCutQueries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "HairMultiCutQueries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "HairMultiCutQueries",
                type: "text",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "LocationId",
                table: "HairMultiCutQueries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "HairMultiCutQueries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "HairMultiCutQueries",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "HairCutQueries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "HairCutQueries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "HairCutQueries",
                type: "text",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "LocationId",
                table: "HairCutQueries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "HairCutQueries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalInfo",
                table: "HairCutQueries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "HairCutQueries",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "EstimateTemplates",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "EstimateTemplates",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "EstimateTemplates",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Estimates",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Estimates",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Estimates",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AttachedGraphicsString",
                table: "Estimates",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AttachedFilesString",
                table: "Estimates",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Estimates",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Secret",
                table: "Applications",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RedirectUri",
                table: "Applications",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LogoutRedirectUri",
                table: "Applications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "Applications",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "Blogspot",
                type: "text",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "Blogspot",
                type: "text",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Blogspot",
                type: "text",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Photo",
                table: "Blogspot",
                type: "text",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Lang",
                table: "Blogspot",
                type: "text",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Blogspot",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AuthorId",
                table: "Blogspot",
                type: "text",
                nullable: true,
                defaultValue: null,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Blogspot",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "BankBook",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "Activities",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "Activities",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SettingsClassName",
                table: "Activities",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Photo",
                table: "Activities",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ParentCode",
                table: "Activities",
                type: "text",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Activities",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModeratorGroupName",
                table: "Activities",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Activities",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserActivities",
                table: "UserActivities",
                columns: new[] { "DoesCode", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SiteSkills",
                table: "SiteSkills",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Services",
                table: "Services",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RdvQueries",
                table: "RdvQueries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Performers",
                table: "Performers",
                column: "PerformerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Locations",
                table: "Locations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HairMultiCutQueries",
                table: "HairMultiCutQueries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HairCutQueries",
                table: "HairCutQueries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExceptionsSIREN",
                table: "ExceptionsSIREN",
                column: "SIREN");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EstimateTemplates",
                table: "EstimateTemplates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Estimates",
                table: "Estimates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Applications",
                table: "Applications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CircleMembers",
                table: "CircleMembers",
                columns: new[] { "MemberId", "CircleId" });
            migrationBuilder.DropPrimaryKey(
                "PK_BlogPost","Blogspot"
            );
             
            migrationBuilder.AddPrimaryKey(
                name: "PK_Blogspot",
                table: "Blogspot",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BankBook",
                table: "BankBook",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Activities",
                table: "Activities",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BankStatus",
                table: "BankStatus",
                column: "UserId");

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

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEvent_PeriodStart_PeriodEnd",
                table: "ScheduledEvent",
                columns: new[] { "PeriodStart", "PeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEvent_ScheduleOwnerId",
                table: "ScheduledEvent",
                column: "ScheduleOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBuildConfiguration_ProjectId",
                table: "ProjectBuildConfiguration",
                column: "ProjectId");

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
                name: "IX_PayPalPayment_ExecutorId",
                table: "PayPalPayment",
                column: "ExecutorId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicalPreference_DjSettingsUserId",
                table: "MusicalPreference",
                column: "DjSettingsUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicalPreference_GeneralSettingsUserId",
                table: "MusicalPreference",
                column: "GeneralSettingsUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveFlow_OwnerId",
                table: "LiveFlow",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentRating_OwnerId",
                table: "InstrumentRating",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Instrumentation_UserId",
                table: "Instrumentation",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HyperLink_BrusherProfileUserId",
                table: "HyperLink",
                column: "BrusherProfileUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HyperLink_PayPalPaymentCreationToken",
                table: "HyperLink",
                column: "PayPalPaymentCreationToken");

            migrationBuilder.CreateIndex(
                name: "IX_HairTaintInstance_PrestationId",
                table: "HairTaintInstance",
                column: "PrestationId");

            migrationBuilder.CreateIndex(
                name: "IX_HairTaint_ColorId",
                table: "HairTaint",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_HairPrestationCollectionItem_PrestationId",
                table: "HairPrestationCollectionItem",
                column: "PrestationId");

            migrationBuilder.CreateIndex(
                name: "IX_HairPrestationCollectionItem_QueryId",
                table: "HairPrestationCollectionItem",
                column: "QueryId");

            migrationBuilder.CreateIndex(
                name: "IX_GitRepositoryReference_OwnerId",
                table: "GitRepositoryReference",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DimissClicked_NotificationId",
                table: "DimissClicked",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceDeclaration_DeviceOwnerId",
                table: "DeviceDeclaration",
                column: "DeviceOwnerId");

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
                name: "IX_Contact_AddressId",
                table: "Contact",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Contact_ApplicationUserId",
                table: "Contact",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_AuthorId",
                table: "Comment",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_ParentId",
                table: "Comment",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_PostId",
                table: "Comment",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_CommandLine_EstimateId",
                table: "CommandLine",
                column: "EstimateId");

            migrationBuilder.CreateIndex(
                name: "IX_CommandLine_EstimateTemplateId",
                table: "CommandLine",
                column: "EstimateTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_CommandForm_ActivityCode",
                table: "CommandForm",
                column: "ActivityCode");

            migrationBuilder.CreateIndex(
                name: "IX_CircleAuthorizationToBlogPost_BlogPostId",
                table: "CircleAuthorizationToBlogPost",
                column: "BlogPostId");

            migrationBuilder.CreateIndex(
                name: "IX_Circle_ApplicationUserId",
                table: "Circle",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomAccess_UserId",
                table: "ChatRoomAccess",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoom_OwnerId",
                table: "ChatRoom",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatConnection_ApplicationUserId",
                table: "ChatConnection",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bug_FeatureId",
                table: "Bug",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_BrusherProfile_ScheduleOwnerId",
                table: "BrusherProfile",
                column: "ScheduleOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogTrad_TraducerId",
                table: "BlogTrad",
                column: "TraducerId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogTag_TagId",
                table: "BlogTag",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_BlackListed_OwnerId",
                table: "BlackListed",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_BlackListed_UserId",
                table: "BlackListed",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ban_TargetId",
                table: "Ban",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BankInfoId",
                table: "AspNetUsers",
                column: "BankInfoId");

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
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Announce_OwnerId",
                table: "Announce",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_UserId",
                table: "UserActivities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ContextId",
                table: "Services",
                column: "ContextId");

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
                name: "IX_Performers_OrganizationAddressId",
                table: "Performers",
                column: "OrganizationAddressId");

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
                name: "IX_CircleMembers_CircleId",
                table: "CircleMembers",
                column: "CircleId");

            migrationBuilder.CreateIndex(
                name: "IX_Blogspot_AuthorId",
                table: "Blogspot",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_BankBook_BalanceId",
                table: "BankBook",
                column: "BalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ParentCode",
                table: "Activities",
                column: "ParentCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Activities_ParentCode",
                table: "Activities",
                column: "ParentCode",
                principalTable: "Activities",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Announce_AspNetUsers_OwnerId",
                table: "Announce",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_BankIdentity_BankInfoId",
                table: "AspNetUsers",
                column: "BankInfoId",
                principalTable: "BankIdentity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Locations_PostalAddressId",
                table: "AspNetUsers",
                column: "PostalAddressId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ban_AspNetUsers_TargetId",
                table: "Ban",
                column: "TargetId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BankBook_BankStatus_BalanceId",
                table: "BankBook",
                column: "BalanceId",
                principalTable: "BankStatus",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BankStatus_AspNetUsers_UserId",
                table: "BankStatus",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BlackListed_AspNetUsers_OwnerId",
                table: "BlackListed",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BlackListed_AspNetUsers_UserId",
                table: "BlackListed",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Blogspot_AspNetUsers_AuthorId",
                table: "Blogspot",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogTag_Blogspot_PostId",
                table: "BlogTag",
                column: "PostId",
                principalTable: "Blogspot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogTag_Tags_TagId",
                table: "BlogTag",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogTrad_AspNetUsers_TraducerId",
                table: "BlogTrad",
                column: "TraducerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BrusherProfile_Performers_UserId",
                table: "BrusherProfile",
                column: "UserId",
                principalTable: "Performers",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BrusherProfile_Schedule_ScheduleOwnerId",
                table: "BrusherProfile",
                column: "ScheduleOwnerId",
                principalTable: "Schedule",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bug_Feature_FeatureId",
                table: "Bug",
                column: "FeatureId",
                principalTable: "Feature",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatConnection_AspNetUsers_ApplicationUserId",
                table: "ChatConnection",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoom_AspNetUsers_OwnerId",
                table: "ChatRoom",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomAccess_AspNetUsers_UserId",
                table: "ChatRoomAccess",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomAccess_ChatRoom_ChannelName",
                table: "ChatRoomAccess",
                column: "ChannelName",
                principalTable: "ChatRoom",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CircleAuthorizationToBlogPost_Blogspot_BlogPostId",
                table: "CircleAuthorizationToBlogPost",
                column: "BlogPostId",
                principalTable: "Blogspot",
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
                name: "FK_CircleMembers_AspNetUsers_MemberId",
                table: "CircleMembers",
                column: "MemberId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CircleMembers_Circle_CircleId",
                table: "CircleMembers",
                column: "CircleId",
                principalTable: "Circle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommandForm_Activities_ActivityCode",
                table: "CommandForm",
                column: "ActivityCode",
                principalTable: "Activities",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommandLine_EstimateTemplates_EstimateTemplateId",
                table: "CommandLine",
                column: "EstimateTemplateId",
                principalTable: "EstimateTemplates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommandLine_Estimates_EstimateId",
                table: "CommandLine",
                column: "EstimateId",
                principalTable: "Estimates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_AspNetUsers_AuthorId",
                table: "Comment",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Blogspot_PostId",
                table: "Comment",
                column: "PostId",
                principalTable: "Blogspot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contact_PostalAddress_AddressId",
                table: "Contact",
                column: "AddressId",
                principalTable: "PostalAddress",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoWorking_AspNetUsers_WorkingForId",
                table: "CoWorking",
                column: "WorkingForId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoWorking_Performers_PerformerId",
                table: "CoWorking",
                column: "PerformerId",
                principalTable: "Performers",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceDeclaration_AspNetUsers_DeviceOwnerId",
                table: "DeviceDeclaration",
                column: "DeviceOwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DimissClicked_AspNetUsers_UserId",
                table: "DimissClicked",
                column: "UserId",
                principalTable: "AspNetUsers",
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
                name: "FK_Estimates_AspNetUsers_ClientId",
                table: "Estimates",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Estimates_Performers_OwnerId",
                table: "Estimates",
                column: "OwnerId",
                principalTable: "Performers",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Estimates_RdvQueries_CommandId",
                table: "Estimates",
                column: "CommandId",
                principalTable: "RdvQueries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GitRepositoryReference_AspNetUsers_OwnerId",
                table: "GitRepositoryReference",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQueries_Activities_ActivityCode",
                table: "HairCutQueries",
                column: "ActivityCode",
                principalTable: "Activities",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQueries_AspNetUsers_ClientId",
                table: "HairCutQueries",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQueries_BrusherProfile_SelectedProfileUserId",
                table: "HairCutQueries",
                column: "SelectedProfileUserId",
                principalTable: "BrusherProfile",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQueries_HairPrestation_PrestationId",
                table: "HairCutQueries",
                column: "PrestationId",
                principalTable: "HairPrestation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQueries_Locations_LocationId",
                table: "HairCutQueries",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQueries_PayPalPayment_PaymentId",
                table: "HairCutQueries",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQueries_Performers_PerformerId",
                table: "HairCutQueries",
                column: "PerformerId",
                principalTable: "Performers",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQueries_Activities_ActivityCode",
                table: "HairMultiCutQueries",
                column: "ActivityCode",
                principalTable: "Activities",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQueries_AspNetUsers_ClientId",
                table: "HairMultiCutQueries",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQueries_Locations_LocationId",
                table: "HairMultiCutQueries",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQueries_PayPalPayment_PaymentId",
                table: "HairMultiCutQueries",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQueries_Performers_PerformerId",
                table: "HairMultiCutQueries",
                column: "PerformerId",
                principalTable: "Performers",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HairPrestationCollectionItem_HairMultiCutQueries_QueryId",
                table: "HairPrestationCollectionItem",
                column: "QueryId",
                principalTable: "HairMultiCutQueries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HairPrestationCollectionItem_HairPrestation_PrestationId",
                table: "HairPrestationCollectionItem",
                column: "PrestationId",
                principalTable: "HairPrestation",
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
                name: "FK_Instrumentation_Instrument_InstrumentId",
                table: "Instrumentation",
                column: "InstrumentId",
                principalTable: "Instrument",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Instrumentation_Performers_UserId",
                table: "Instrumentation",
                column: "UserId",
                principalTable: "Performers",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstrumentRating_Instrument_InstrumentId",
                table: "InstrumentRating",
                column: "InstrumentId",
                principalTable: "Instrument",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstrumentRating_Performers_OwnerId",
                table: "InstrumentRating",
                column: "OwnerId",
                principalTable: "Performers",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LiveFlow_AspNetUsers_OwnerId",
                table: "LiveFlow",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PayPalPayment_AspNetUsers_ExecutorId",
                table: "PayPalPayment",
                column: "ExecutorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Performers_AspNetUsers_PerformerId",
                table: "Performers",
                column: "PerformerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Performers_Locations_OrganizationAddressId",
                table: "Performers",
                column: "OrganizationAddressId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Activities_ActivityCode",
                table: "Project",
                column: "ActivityCode",
                principalTable: "Activities",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_AspNetUsers_ClientId",
                table: "Project",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_GitRepositoryReference_GitId",
                table: "Project",
                column: "GitId",
                principalTable: "GitRepositoryReference",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_PayPalPayment_PaymentId",
                table: "Project",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Performers_PerformerId",
                table: "Project",
                column: "PerformerId",
                principalTable: "Performers",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectBuildConfiguration_Project_ProjectId",
                table: "ProjectBuildConfiguration",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RdvQueries_Activities_ActivityCode",
                table: "RdvQueries",
                column: "ActivityCode",
                principalTable: "Activities",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RdvQueries_AspNetUsers_ClientId",
                table: "RdvQueries",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RdvQueries_Locations_LocationId",
                table: "RdvQueries",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RdvQueries_PayPalPayment_PaymentId",
                table: "RdvQueries",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RdvQueries_Performers_PerformerId",
                table: "RdvQueries",
                column: "PerformerId",
                principalTable: "Performers",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_AspNetUsers_OwnerId",
                table: "Schedule",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledEvent_Period_PeriodStart_PeriodEnd",
                table: "ScheduledEvent",
                columns: new[] { "PeriodStart", "PeriodEnd" },
                principalTable: "Period",
                principalColumns: new[] { "Start", "End" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Activities_ContextId",
                table: "Services",
                column: "ContextId",
                principalTable: "Activities",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivities_Activities_DoesCode",
                table: "UserActivities",
                column: "DoesCode",
                principalTable: "Activities",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivities_Performers_UserId",
                table: "UserActivities",
                column: "UserId",
                principalTable: "Performers",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Activities_ParentCode",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Announce_AspNetUsers_OwnerId",
                table: "Announce");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_BankIdentity_BankInfoId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Locations_PostalAddressId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Ban_AspNetUsers_TargetId",
                table: "Ban");

            migrationBuilder.DropForeignKey(
                name: "FK_BankBook_BankStatus_BalanceId",
                table: "BankBook");

            migrationBuilder.DropForeignKey(
                name: "FK_BankStatus_AspNetUsers_UserId",
                table: "BankStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_BlackListed_AspNetUsers_OwnerId",
                table: "BlackListed");

            migrationBuilder.DropForeignKey(
                name: "FK_BlackListed_AspNetUsers_UserId",
                table: "BlackListed");

            migrationBuilder.DropForeignKey(
                name: "FK_Blogspot_AspNetUsers_AuthorId",
                table: "Blogspot");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogTag_Blogspot_PostId",
                table: "BlogTag");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogTag_Tags_TagId",
                table: "BlogTag");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogTrad_AspNetUsers_TraducerId",
                table: "BlogTrad");

            migrationBuilder.DropForeignKey(
                name: "FK_BrusherProfile_Performers_UserId",
                table: "BrusherProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_BrusherProfile_Schedule_ScheduleOwnerId",
                table: "BrusherProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_Bug_Feature_FeatureId",
                table: "Bug");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatConnection_AspNetUsers_ApplicationUserId",
                table: "ChatConnection");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoom_AspNetUsers_OwnerId",
                table: "ChatRoom");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoomAccess_AspNetUsers_UserId",
                table: "ChatRoomAccess");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoomAccess_ChatRoom_ChannelName",
                table: "ChatRoomAccess");

            migrationBuilder.DropForeignKey(
                name: "FK_CircleAuthorizationToBlogPost_Blogspot_BlogPostId",
                table: "CircleAuthorizationToBlogPost");

            migrationBuilder.DropForeignKey(
                name: "FK_CircleAuthorizationToBlogPost_Circle_CircleId",
                table: "CircleAuthorizationToBlogPost");

            migrationBuilder.DropForeignKey(
                name: "FK_CircleMembers_AspNetUsers_MemberId",
                table: "CircleMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_CircleMembers_Circle_CircleId",
                table: "CircleMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_CommandForm_Activities_ActivityCode",
                table: "CommandForm");

            migrationBuilder.DropForeignKey(
                name: "FK_CommandLine_EstimateTemplates_EstimateTemplateId",
                table: "CommandLine");

            migrationBuilder.DropForeignKey(
                name: "FK_CommandLine_Estimates_EstimateId",
                table: "CommandLine");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_AspNetUsers_AuthorId",
                table: "Comment");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Blogspot_PostId",
                table: "Comment");

            migrationBuilder.DropForeignKey(
                name: "FK_Contact_PostalAddress_AddressId",
                table: "Contact");

            migrationBuilder.DropForeignKey(
                name: "FK_CoWorking_AspNetUsers_WorkingForId",
                table: "CoWorking");

            migrationBuilder.DropForeignKey(
                name: "FK_CoWorking_Performers_PerformerId",
                table: "CoWorking");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceDeclaration_AspNetUsers_DeviceOwnerId",
                table: "DeviceDeclaration");

            migrationBuilder.DropForeignKey(
                name: "FK_DimissClicked_AspNetUsers_UserId",
                table: "DimissClicked");

            migrationBuilder.DropForeignKey(
                name: "FK_DimissClicked_Notification_NotificationId",
                table: "DimissClicked");

            migrationBuilder.DropForeignKey(
                name: "FK_Estimates_AspNetUsers_ClientId",
                table: "Estimates");

            migrationBuilder.DropForeignKey(
                name: "FK_Estimates_Performers_OwnerId",
                table: "Estimates");

            migrationBuilder.DropForeignKey(
                name: "FK_Estimates_RdvQueries_CommandId",
                table: "Estimates");

            migrationBuilder.DropForeignKey(
                name: "FK_GitRepositoryReference_AspNetUsers_OwnerId",
                table: "GitRepositoryReference");

            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQueries_Activities_ActivityCode",
                table: "HairCutQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQueries_AspNetUsers_ClientId",
                table: "HairCutQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQueries_BrusherProfile_SelectedProfileUserId",
                table: "HairCutQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQueries_HairPrestation_PrestationId",
                table: "HairCutQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQueries_Locations_LocationId",
                table: "HairCutQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQueries_PayPalPayment_PaymentId",
                table: "HairCutQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQueries_Performers_PerformerId",
                table: "HairCutQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_HairMultiCutQueries_Activities_ActivityCode",
                table: "HairMultiCutQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_HairMultiCutQueries_AspNetUsers_ClientId",
                table: "HairMultiCutQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_HairMultiCutQueries_Locations_LocationId",
                table: "HairMultiCutQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_HairMultiCutQueries_PayPalPayment_PaymentId",
                table: "HairMultiCutQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_HairMultiCutQueries_Performers_PerformerId",
                table: "HairMultiCutQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_HairPrestationCollectionItem_HairMultiCutQueries_QueryId",
                table: "HairPrestationCollectionItem");

            migrationBuilder.DropForeignKey(
                name: "FK_HairPrestationCollectionItem_HairPrestation_PrestationId",
                table: "HairPrestationCollectionItem");

            migrationBuilder.DropForeignKey(
                name: "FK_HairTaint_Color_ColorId",
                table: "HairTaint");

            migrationBuilder.DropForeignKey(
                name: "FK_HairTaintInstance_HairPrestation_PrestationId",
                table: "HairTaintInstance");

            migrationBuilder.DropForeignKey(
                name: "FK_HairTaintInstance_HairTaint_TaintId",
                table: "HairTaintInstance");

            migrationBuilder.DropForeignKey(
                name: "FK_Instrumentation_Instrument_InstrumentId",
                table: "Instrumentation");

            migrationBuilder.DropForeignKey(
                name: "FK_Instrumentation_Performers_UserId",
                table: "Instrumentation");

            migrationBuilder.DropForeignKey(
                name: "FK_InstrumentRating_Instrument_InstrumentId",
                table: "InstrumentRating");

            migrationBuilder.DropForeignKey(
                name: "FK_InstrumentRating_Performers_OwnerId",
                table: "InstrumentRating");

            migrationBuilder.DropForeignKey(
                name: "FK_LiveFlow_AspNetUsers_OwnerId",
                table: "LiveFlow");

            migrationBuilder.DropForeignKey(
                name: "FK_PayPalPayment_AspNetUsers_ExecutorId",
                table: "PayPalPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_Performers_AspNetUsers_PerformerId",
                table: "Performers");

            migrationBuilder.DropForeignKey(
                name: "FK_Performers_Locations_OrganizationAddressId",
                table: "Performers");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Activities_ActivityCode",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_AspNetUsers_ClientId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_GitRepositoryReference_GitId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_PayPalPayment_PaymentId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Performers_PerformerId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectBuildConfiguration_Project_ProjectId",
                table: "ProjectBuildConfiguration");

            migrationBuilder.DropForeignKey(
                name: "FK_RdvQueries_Activities_ActivityCode",
                table: "RdvQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_RdvQueries_AspNetUsers_ClientId",
                table: "RdvQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_RdvQueries_Locations_LocationId",
                table: "RdvQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_RdvQueries_PayPalPayment_PaymentId",
                table: "RdvQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_RdvQueries_Performers_PerformerId",
                table: "RdvQueries");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_AspNetUsers_OwnerId",
                table: "Schedule");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledEvent_Period_PeriodStart_PeriodEnd",
                table: "ScheduledEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Activities_ContextId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActivities_Activities_DoesCode",
                table: "UserActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActivities_Performers_UserId",
                table: "UserActivities");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropIndex(
                name: "IX_ScheduledEvent_PeriodStart_PeriodEnd",
                table: "ScheduledEvent");

            migrationBuilder.DropIndex(
                name: "IX_ScheduledEvent_ScheduleOwnerId",
                table: "ScheduledEvent");

            migrationBuilder.DropIndex(
                name: "IX_ProjectBuildConfiguration_ProjectId",
                table: "ProjectBuildConfiguration");

            migrationBuilder.DropIndex(
                name: "IX_Project_ActivityCode",
                table: "Project");

            migrationBuilder.DropIndex(
                name: "IX_Project_ClientId",
                table: "Project");

            migrationBuilder.DropIndex(
                name: "IX_Project_GitId",
                table: "Project");

            migrationBuilder.DropIndex(
                name: "IX_Project_PaymentId",
                table: "Project");

            migrationBuilder.DropIndex(
                name: "IX_Project_PerformerId",
                table: "Project");

            migrationBuilder.DropIndex(
                name: "IX_PayPalPayment_ExecutorId",
                table: "PayPalPayment");

            migrationBuilder.DropIndex(
                name: "IX_MusicalPreference_DjSettingsUserId",
                table: "MusicalPreference");

            migrationBuilder.DropIndex(
                name: "IX_MusicalPreference_GeneralSettingsUserId",
                table: "MusicalPreference");

            migrationBuilder.DropIndex(
                name: "IX_LiveFlow_OwnerId",
                table: "LiveFlow");

            migrationBuilder.DropIndex(
                name: "IX_InstrumentRating_OwnerId",
                table: "InstrumentRating");

            migrationBuilder.DropIndex(
                name: "IX_Instrumentation_UserId",
                table: "Instrumentation");

            migrationBuilder.DropIndex(
                name: "IX_HyperLink_BrusherProfileUserId",
                table: "HyperLink");

            migrationBuilder.DropIndex(
                name: "IX_HyperLink_PayPalPaymentCreationToken",
                table: "HyperLink");

            migrationBuilder.DropIndex(
                name: "IX_HairTaintInstance_PrestationId",
                table: "HairTaintInstance");

            migrationBuilder.DropIndex(
                name: "IX_HairTaint_ColorId",
                table: "HairTaint");

            migrationBuilder.DropIndex(
                name: "IX_HairPrestationCollectionItem_PrestationId",
                table: "HairPrestationCollectionItem");

            migrationBuilder.DropIndex(
                name: "IX_HairPrestationCollectionItem_QueryId",
                table: "HairPrestationCollectionItem");

            migrationBuilder.DropIndex(
                name: "IX_GitRepositoryReference_OwnerId",
                table: "GitRepositoryReference");

            migrationBuilder.DropIndex(
                name: "IX_DimissClicked_NotificationId",
                table: "DimissClicked");

            migrationBuilder.DropIndex(
                name: "IX_DeviceDeclaration_DeviceOwnerId",
                table: "DeviceDeclaration");

            migrationBuilder.DropIndex(
                name: "IX_CoWorking_FormationSettingsUserId",
                table: "CoWorking");

            migrationBuilder.DropIndex(
                name: "IX_CoWorking_PerformerId",
                table: "CoWorking");

            migrationBuilder.DropIndex(
                name: "IX_CoWorking_WorkingForId",
                table: "CoWorking");

            migrationBuilder.DropIndex(
                name: "IX_Contact_AddressId",
                table: "Contact");

            migrationBuilder.DropIndex(
                name: "IX_Contact_ApplicationUserId",
                table: "Contact");

            migrationBuilder.DropIndex(
                name: "IX_Comment_AuthorId",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_ParentId",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_Comment_PostId",
                table: "Comment");

            migrationBuilder.DropIndex(
                name: "IX_CommandLine_EstimateId",
                table: "CommandLine");

            migrationBuilder.DropIndex(
                name: "IX_CommandLine_EstimateTemplateId",
                table: "CommandLine");

            migrationBuilder.DropIndex(
                name: "IX_CommandForm_ActivityCode",
                table: "CommandForm");

            migrationBuilder.DropIndex(
                name: "IX_CircleAuthorizationToBlogPost_BlogPostId",
                table: "CircleAuthorizationToBlogPost");

            migrationBuilder.DropIndex(
                name: "IX_Circle_ApplicationUserId",
                table: "Circle");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoomAccess_UserId",
                table: "ChatRoomAccess");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoom_OwnerId",
                table: "ChatRoom");

            migrationBuilder.DropIndex(
                name: "IX_ChatConnection_ApplicationUserId",
                table: "ChatConnection");

            migrationBuilder.DropIndex(
                name: "IX_Bug_FeatureId",
                table: "Bug");

            migrationBuilder.DropIndex(
                name: "IX_BrusherProfile_ScheduleOwnerId",
                table: "BrusherProfile");

            migrationBuilder.DropIndex(
                name: "IX_BlogTrad_TraducerId",
                table: "BlogTrad");

            migrationBuilder.DropIndex(
                name: "IX_BlogTag_TagId",
                table: "BlogTag");

            migrationBuilder.DropIndex(
                name: "IX_BlackListed_OwnerId",
                table: "BlackListed");

            migrationBuilder.DropIndex(
                name: "IX_BlackListed_UserId",
                table: "BlackListed");

            migrationBuilder.DropIndex(
                name: "IX_Ban_TargetId",
                table: "Ban");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_BankInfoId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PostalAddressId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropIndex(
                name: "IX_Announce_OwnerId",
                table: "Announce");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserActivities",
                table: "UserActivities");

            migrationBuilder.DropIndex(
                name: "IX_UserActivities_UserId",
                table: "UserActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SiteSkills",
                table: "SiteSkills");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Services",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_ContextId",
                table: "Services");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RdvQueries",
                table: "RdvQueries");

            migrationBuilder.DropIndex(
                name: "IX_RdvQueries_ActivityCode",
                table: "RdvQueries");

            migrationBuilder.DropIndex(
                name: "IX_RdvQueries_ClientId",
                table: "RdvQueries");

            migrationBuilder.DropIndex(
                name: "IX_RdvQueries_LocationId",
                table: "RdvQueries");

            migrationBuilder.DropIndex(
                name: "IX_RdvQueries_PaymentId",
                table: "RdvQueries");

            migrationBuilder.DropIndex(
                name: "IX_RdvQueries_PerformerId",
                table: "RdvQueries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Performers",
                table: "Performers");

            migrationBuilder.DropIndex(
                name: "IX_Performers_OrganizationAddressId",
                table: "Performers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Locations",
                table: "Locations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HairMultiCutQueries",
                table: "HairMultiCutQueries");

            migrationBuilder.DropIndex(
                name: "IX_HairMultiCutQueries_ActivityCode",
                table: "HairMultiCutQueries");

            migrationBuilder.DropIndex(
                name: "IX_HairMultiCutQueries_ClientId",
                table: "HairMultiCutQueries");

            migrationBuilder.DropIndex(
                name: "IX_HairMultiCutQueries_LocationId",
                table: "HairMultiCutQueries");

            migrationBuilder.DropIndex(
                name: "IX_HairMultiCutQueries_PaymentId",
                table: "HairMultiCutQueries");

            migrationBuilder.DropIndex(
                name: "IX_HairMultiCutQueries_PerformerId",
                table: "HairMultiCutQueries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HairCutQueries",
                table: "HairCutQueries");

            migrationBuilder.DropIndex(
                name: "IX_HairCutQueries_ActivityCode",
                table: "HairCutQueries");

            migrationBuilder.DropIndex(
                name: "IX_HairCutQueries_ClientId",
                table: "HairCutQueries");

            migrationBuilder.DropIndex(
                name: "IX_HairCutQueries_LocationId",
                table: "HairCutQueries");

            migrationBuilder.DropIndex(
                name: "IX_HairCutQueries_PaymentId",
                table: "HairCutQueries");

            migrationBuilder.DropIndex(
                name: "IX_HairCutQueries_PerformerId",
                table: "HairCutQueries");

            migrationBuilder.DropIndex(
                name: "IX_HairCutQueries_PrestationId",
                table: "HairCutQueries");

            migrationBuilder.DropIndex(
                name: "IX_HairCutQueries_SelectedProfileUserId",
                table: "HairCutQueries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExceptionsSIREN",
                table: "ExceptionsSIREN");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EstimateTemplates",
                table: "EstimateTemplates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Estimates",
                table: "Estimates");

            migrationBuilder.DropIndex(
                name: "IX_Estimates_ClientId",
                table: "Estimates");

            migrationBuilder.DropIndex(
                name: "IX_Estimates_CommandId",
                table: "Estimates");

            migrationBuilder.DropIndex(
                name: "IX_Estimates_OwnerId",
                table: "Estimates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CircleMembers",
                table: "CircleMembers");

            migrationBuilder.DropIndex(
                name: "IX_CircleMembers_CircleId",
                table: "CircleMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Blogspot",
                table: "Blogspot");

            migrationBuilder.DropIndex(
                name: "IX_Blogspot_AuthorId",
                table: "Blogspot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BankStatus",
                table: "BankStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BankBook",
                table: "BankBook");

            migrationBuilder.DropIndex(
                name: "IX_BankBook_BalanceId",
                table: "BankBook");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Applications",
                table: "Applications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Activities",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_ParentCode",
                table: "Activities");

            migrationBuilder.RenameTable(
                name: "UserActivities",
                newName: "UserActivity");

            migrationBuilder.RenameTable(
                name: "Tags",
                newName: "Tag");

            migrationBuilder.RenameTable(
                name: "SiteSkills",
                newName: "Skill");

            migrationBuilder.RenameTable(
                name: "Services",
                newName: "Service");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                newName: "RefreshToken");

            migrationBuilder.RenameTable(
                name: "RdvQueries",
                newName: "RdvQuery");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Product");

            migrationBuilder.RenameTable(
                name: "Performers",
                newName: "PerformerProfile");

            migrationBuilder.RenameTable(
                name: "Locations",
                newName: "Location");

            migrationBuilder.RenameTable(
                name: "HairMultiCutQueries",
                newName: "HairMultiCutQuery");

            migrationBuilder.RenameTable(
                name: "HairCutQueries",
                newName: "HairCutQuery");

            migrationBuilder.RenameTable(
                name: "ExceptionsSIREN",
                newName: "ExceptionSIREN");

            migrationBuilder.RenameTable(
                name: "EstimateTemplates",
                newName: "EstimateTemplate");

            migrationBuilder.RenameTable(
                name: "Estimates",
                newName: "Estimate");

            migrationBuilder.RenameTable(
                name: "CircleMembers",
                newName: "CircleMember");

            migrationBuilder.RenameTable(
                name: "Blogspot",
                newName: "BlogPost");

            migrationBuilder.RenameTable(
                name: "BankStatus",
                newName: "AccountBalance");

            migrationBuilder.RenameTable(
                name: "BankBook",
                newName: "BalanceImpact");

            migrationBuilder.RenameTable(
                name: "Applications",
                newName: "Client");

            migrationBuilder.RenameTable(
                name: "Activities",
                newName: "Activity");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PeriodStart",
                table: "ScheduledEvent",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PeriodEnd",
                table: "ScheduledEvent",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "ScheduledEvent",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProjectBuildConfiguration",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "ProjectBuildConfiguration",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "Project",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "Project",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "Project",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "Project",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Project",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Project",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Project",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Project",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Street2",
                table: "PostalAddress",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Street1",
                table: "PostalAddress",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "PostalAddress",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Province",
                table: "PostalAddress",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "PostalAddress",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "PostalAddress",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "PostalAddress",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "PostalAddress",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "PayPalPayment",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "PayPalPayment",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "PayPalPayment",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PaypalPayerId",
                table: "PayPalPayment",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OrderReference",
                table: "PayPalPayment",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ExecutorId",
                table: "PayPalPayment",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "Option",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "Option",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Option",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "TokenType",
                table: "OAuth2Tokens",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "OAuth2Tokens",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ExpiresIn",
                table: "OAuth2Tokens",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "OAuth2Tokens",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "Notification",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024);

            migrationBuilder.AlterColumn<string>(
                name: "tag",
                table: "Notification",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "sound",
                table: "Notification",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "icon",
                table: "Notification",
                type: "text",
                nullable: true,
                defaultValue: "exclam",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldDefaultValue: "exclam");

            migrationBuilder.AlterColumn<string>(
                name: "color",
                table: "Notification",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "click_action",
                table: "Notification",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "body",
                table: "Notification",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "Target",
                table: "Notification",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Notification",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "MusicalTendency",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "MusicalTendency",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "MailingTemplate",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "MailingTemplate",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "MailingTemplate",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ReplyToAddress",
                table: "MailingTemplate",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Body",
                table: "MailingTemplate",
                type: "character varying(65536)",
                maxLength: 65536,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(65536)",
                oldMaxLength: 65536);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "LiveFlow",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Pitch",
                table: "LiveFlow",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "LiveFlow",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MediaType",
                table: "LiveFlow",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DifferedFileName",
                table: "LiveFlow",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "LiveFlow",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "InstrumentRating",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Instrument",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Instrument",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Rel",
                table: "HyperLink",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "HyperLink",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "HairTaint",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "HairTaint",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "HairPrestationCollectionItem",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "HairPrestation",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "GitRepositoryReference",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "GitRepositoryReference",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "GitRepositoryReference",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Branch",
                table: "GitRepositoryReference",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "GitRepositoryReference",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "FormationSettings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "Form",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ShortName",
                table: "Feature",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Feature",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Feature",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "SoundCloudId",
                table: "DjSettings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "DeviceDeclaration",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Platform",
                table: "DeviceDeclaration",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "DeviceDeclaration",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceOwnerId",
                table: "DeviceDeclaration",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeclarationDate",
                table: "DeviceDeclaration",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "LOCALTIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "WorkingForId",
                table: "CoWorking",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PerformerId",
                table: "CoWorking",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "CoWorking",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Contact",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "EMail",
                table: "Contact",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "Comment",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "Comment",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Comment",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Comment",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "CommandLine",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "CommandLine",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "CommandForm",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ActivityCode",
                table: "CommandForm",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ActionName",
                table: "CommandForm",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "CommandForm",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Color",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Color",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "ClientProviderInfo",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "ClientProviderInfo",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "EMail",
                table: "ClientProviderInfo",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Avatar",
                table: "ClientProviderInfo",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Circle",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Circle",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Circle",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "ChatRoomAccess",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "ChatRoom",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "ChatRoom",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "ChatRoom",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "ChatRoom",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "ChatConnection",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Bug",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "FeatureId",
                table: "Bug",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Bug",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Bug",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "ScheduleOwnerId",
                table: "BrusherProfile",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "TraducerId",
                table: "BlogTrad",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "BlogTrad",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Body",
                table: "BlogTrad",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "BlackListed",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "WicketCode",
                table: "BankIdentity",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "IBAN",
                table: "BankIdentity",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "BankCode",
                table: "BankIdentity",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "BIC",
                table: "BankIdentity",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "BankIdentity",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "BankIdentity",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "Ban",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "Ban",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Ban",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "PostalAddressId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "DedicatedGoogleCalendar",
                table: "AspNetUsers",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<long>(
                name: "BankInfoId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Avatar",
                table: "AspNetUsers",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                defaultValue: "/images/Users/icon_user.png",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldDefaultValue: "/images/Users/icon_user.png");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "AspNetUserClaims",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "AspNetRoleClaims",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "Announce",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Sender",
                table: "Announce",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Announce",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Announce",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Announce",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tag",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Tag",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Skill",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Skill",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Service",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Service",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ContextId",
                table: "Service",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Service",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "RdvQuery",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "RdvQuery",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "RdvQuery",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "RdvQuery",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "LocationId",
                table: "RdvQuery",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "RdvQuery",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "RdvQuery",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Product",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Product",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Product",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "WebSite",
                table: "PerformerProfile",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "SIREN",
                table: "PerformerProfile",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Location",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Location",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "HairMultiCutQuery",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "HairMultiCutQuery",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "HairMultiCutQuery",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "LocationId",
                table: "HairMultiCutQuery",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "HairMultiCutQuery",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "HairMultiCutQuery",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "HairCutQuery",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "HairCutQuery",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentId",
                table: "HairCutQuery",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "LocationId",
                table: "HairCutQuery",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "HairCutQuery",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalInfo",
                table: "HairCutQuery",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "HairCutQuery",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "EstimateTemplate",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "EstimateTemplate",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "EstimateTemplate",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Estimate",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Estimate",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Estimate",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AttachedGraphicsString",
                table: "Estimate",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AttachedFilesString",
                table: "Estimate",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Estimate",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "BlogPost",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "BlogPost",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "BlogPost",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Photo",
                table: "BlogPost",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Lang",
                table: "BlogPost",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "BlogPost",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AuthorId",
                table: "BlogPost",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "BlogPost",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "BalanceImpact",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Secret",
                table: "Client",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "RedirectUri",
                table: "Client",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LogoutRedirectUri",
                table: "Client",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "Client",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserModified",
                table: "Activity",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserCreated",
                table: "Activity",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "SettingsClassName",
                table: "Activity",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Photo",
                table: "Activity",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ParentCode",
                table: "Activity",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Activity",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ModeratorGroupName",
                table: "Activity",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Activity",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserActivity",
                table: "UserActivity",
                columns: new[] { "DoesCode", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tag",
                table: "Tag",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Skill",
                table: "Skill",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Service",
                table: "Service",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshToken",
                table: "RefreshToken",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RdvQuery",
                table: "RdvQuery",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Product",
                table: "Product",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PerformerProfile",
                table: "PerformerProfile",
                column: "PerformerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Location",
                table: "Location",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HairMultiCutQuery",
                table: "HairMultiCutQuery",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HairCutQuery",
                table: "HairCutQuery",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExceptionSIREN",
                table: "ExceptionSIREN",
                column: "SIREN");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EstimateTemplate",
                table: "EstimateTemplate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Estimate",
                table: "Estimate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CircleMember",
                table: "CircleMember",
                columns: new[] { "MemberId", "CircleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogPost",
                table: "BlogPost",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccountBalance",
                table: "AccountBalance",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BalanceImpact",
                table: "BalanceImpact",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Client",
                table: "Client",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Activity",
                table: "Activity",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountBalance_AspNetUsers_UserId",
                table: "AccountBalance",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_Activity_ParentCode",
                table: "Activity",
                column: "ParentCode",
                principalTable: "Activity",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Announce_AspNetUsers_OwnerId",
                table: "Announce",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_BankIdentity_BankInfoId",
                table: "AspNetUsers",
                column: "BankInfoId",
                principalTable: "BankIdentity",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Location_PostalAddressId",
                table: "AspNetUsers",
                column: "PostalAddressId",
                principalTable: "Location",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BalanceImpact_AccountBalance_BalanceId",
                table: "BalanceImpact",
                column: "BalanceId",
                principalTable: "AccountBalance",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ban_AspNetUsers_TargetId",
                table: "Ban",
                column: "TargetId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlackListed_AspNetUsers_OwnerId",
                table: "BlackListed",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlackListed_AspNetUsers_UserId",
                table: "BlackListed",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPost_AspNetUsers_AuthorId",
                table: "BlogPost",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogTag_BlogPost_PostId",
                table: "BlogTag",
                column: "PostId",
                principalTable: "BlogPost",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogTag_Tag_TagId",
                table: "BlogTag",
                column: "TagId",
                principalTable: "Tag",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogTrad_AspNetUsers_TraducerId",
                table: "BlogTrad",
                column: "TraducerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BrusherProfile_PerformerProfile_UserId",
                table: "BrusherProfile",
                column: "UserId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId");

            migrationBuilder.AddForeignKey(
                name: "FK_BrusherProfile_Schedule_ScheduleOwnerId",
                table: "BrusherProfile",
                column: "ScheduleOwnerId",
                principalTable: "Schedule",
                principalColumn: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bug_Feature_FeatureId",
                table: "Bug",
                column: "FeatureId",
                principalTable: "Feature",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatConnection_AspNetUsers_ApplicationUserId",
                table: "ChatConnection",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoom_AspNetUsers_OwnerId",
                table: "ChatRoom",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomAccess_AspNetUsers_UserId",
                table: "ChatRoomAccess",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomAccess_ChatRoom_ChannelName",
                table: "ChatRoomAccess",
                column: "ChannelName",
                principalTable: "ChatRoom",
                principalColumn: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_CircleAuthorizationToBlogPost_BlogPost_BlogPostId",
                table: "CircleAuthorizationToBlogPost",
                column: "BlogPostId",
                principalTable: "BlogPost",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CircleAuthorizationToBlogPost_Circle_CircleId",
                table: "CircleAuthorizationToBlogPost",
                column: "CircleId",
                principalTable: "Circle",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CircleMember_AspNetUsers_MemberId",
                table: "CircleMember",
                column: "MemberId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CircleMember_Circle_CircleId",
                table: "CircleMember",
                column: "CircleId",
                principalTable: "Circle",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommandForm_Activity_ActivityCode",
                table: "CommandForm",
                column: "ActivityCode",
                principalTable: "Activity",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_CommandLine_EstimateTemplate_EstimateTemplateId",
                table: "CommandLine",
                column: "EstimateTemplateId",
                principalTable: "EstimateTemplate",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommandLine_Estimate_EstimateId",
                table: "CommandLine",
                column: "EstimateId",
                principalTable: "Estimate",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_AspNetUsers_AuthorId",
                table: "Comment",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_BlogPost_PostId",
                table: "Comment",
                column: "PostId",
                principalTable: "BlogPost",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contact_PostalAddress_AddressId",
                table: "Contact",
                column: "AddressId",
                principalTable: "PostalAddress",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CoWorking_AspNetUsers_WorkingForId",
                table: "CoWorking",
                column: "WorkingForId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CoWorking_PerformerProfile_PerformerId",
                table: "CoWorking",
                column: "PerformerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceDeclaration_AspNetUsers_DeviceOwnerId",
                table: "DeviceDeclaration",
                column: "DeviceOwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DimissClicked_AspNetUsers_UserId",
                table: "DimissClicked",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DimissClicked_Notification_NotificationId",
                table: "DimissClicked",
                column: "NotificationId",
                principalTable: "Notification",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Estimate_AspNetUsers_ClientId",
                table: "Estimate",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Estimate_PerformerProfile_OwnerId",
                table: "Estimate",
                column: "OwnerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Estimate_RdvQuery_CommandId",
                table: "Estimate",
                column: "CommandId",
                principalTable: "RdvQuery",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GitRepositoryReference_AspNetUsers_OwnerId",
                table: "GitRepositoryReference",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQuery_Activity_ActivityCode",
                table: "HairCutQuery",
                column: "ActivityCode",
                principalTable: "Activity",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQuery_AspNetUsers_ClientId",
                table: "HairCutQuery",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQuery_BrusherProfile_SelectedProfileUserId",
                table: "HairCutQuery",
                column: "SelectedProfileUserId",
                principalTable: "BrusherProfile",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQuery_HairPrestation_PrestationId",
                table: "HairCutQuery",
                column: "PrestationId",
                principalTable: "HairPrestation",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQuery_Location_LocationId",
                table: "HairCutQuery",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQuery_PayPalPayment_PaymentId",
                table: "HairCutQuery",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken"
                );

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQuery_PerformerProfile_PerformerId",
                table: "HairCutQuery",
                column: "PerformerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId");

            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQuery_Activity_ActivityCode",
                table: "HairMultiCutQuery",
                column: "ActivityCode",
                principalTable: "Activity",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQuery_AspNetUsers_ClientId",
                table: "HairMultiCutQuery",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQuery_Location_LocationId",
                table: "HairMultiCutQuery",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQuery_PayPalPayment_PaymentId",
                table: "HairMultiCutQuery",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken");

            migrationBuilder.AddForeignKey(
                name: "FK_HairMultiCutQuery_PerformerProfile_PerformerId",
                table: "HairMultiCutQuery",
                column: "PerformerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId");

            migrationBuilder.AddForeignKey(
                name: "FK_HairPrestationCollectionItem_HairMultiCutQuery_QueryId",
                table: "HairPrestationCollectionItem",
                column: "QueryId",
                principalTable: "HairMultiCutQuery",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HairPrestationCollectionItem_HairPrestation_PrestationId",
                table: "HairPrestationCollectionItem",
                column: "PrestationId",
                principalTable: "HairPrestation",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HairTaint_Color_ColorId",
                table: "HairTaint",
                column: "ColorId",
                principalTable: "Color",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HairTaintInstance_HairPrestation_PrestationId",
                table: "HairTaintInstance",
                column: "PrestationId",
                principalTable: "HairPrestation",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HairTaintInstance_HairTaint_TaintId",
                table: "HairTaintInstance",
                column: "TaintId",
                principalTable: "HairTaint",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Instrumentation_Instrument_InstrumentId",
                table: "Instrumentation",
                column: "InstrumentId",
                principalTable: "Instrument",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Instrumentation_PerformerProfile_UserId",
                table: "Instrumentation",
                column: "UserId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId");

            migrationBuilder.AddForeignKey(
                name: "FK_InstrumentRating_Instrument_InstrumentId",
                table: "InstrumentRating",
                column: "InstrumentId",
                principalTable: "Instrument",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InstrumentRating_PerformerProfile_OwnerId",
                table: "InstrumentRating",
                column: "OwnerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId");

            migrationBuilder.AddForeignKey(
                name: "FK_LiveFlow_AspNetUsers_OwnerId",
                table: "LiveFlow",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PayPalPayment_AspNetUsers_ExecutorId",
                table: "PayPalPayment",
                column: "ExecutorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PerformerProfile_AspNetUsers_PerformerId",
                table: "PerformerProfile",
                column: "PerformerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PerformerProfile_Location_OrganizationAddressId",
                table: "PerformerProfile",
                column: "OrganizationAddressId",
                principalTable: "Location",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Activity_ActivityCode",
                table: "Project",
                column: "ActivityCode",
                principalTable: "Activity",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_AspNetUsers_ClientId",
                table: "Project",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_GitRepositoryReference_GitId",
                table: "Project",
                column: "GitId",
                principalTable: "GitRepositoryReference",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_PayPalPayment_PaymentId",
                table: "Project",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_PerformerProfile_PerformerId",
                table: "Project",
                column: "PerformerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectBuildConfiguration_Project_ProjectId",
                table: "ProjectBuildConfiguration",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RdvQuery_Activity_ActivityCode",
                table: "RdvQuery",
                column: "ActivityCode",
                principalTable: "Activity",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_RdvQuery_AspNetUsers_ClientId",
                table: "RdvQuery",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RdvQuery_Location_LocationId",
                table: "RdvQuery",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RdvQuery_PayPalPayment_PaymentId",
                table: "RdvQuery",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken");

            migrationBuilder.AddForeignKey(
                name: "FK_RdvQuery_PerformerProfile_PerformerId",
                table: "RdvQuery",
                column: "PerformerId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_AspNetUsers_OwnerId",
                table: "Schedule",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledEvent_Period_PeriodStart_PeriodEnd",
                table: "ScheduledEvent",
                columns: new[] { "PeriodStart", "PeriodEnd" },
                principalTable: "Period",
                principalColumns: new[] { "Start", "End" });

            migrationBuilder.AddForeignKey(
                name: "FK_Service_Activity_ContextId",
                table: "Service",
                column: "ContextId",
                principalTable: "Activity",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivity_Activity_DoesCode",
                table: "UserActivity",
                column: "DoesCode",
                principalTable: "Activity",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivity_PerformerProfile_UserId",
                table: "UserActivity",
                column: "UserId",
                principalTable: "PerformerProfile",
                principalColumn: "PerformerId");
        }
    }
}
