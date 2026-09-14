using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class NominativeServiceCommand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estimates_NominativeServiceCommand_CommandId",
                table: "Estimates");

            migrationBuilder.DropForeignKey(
                name: "FK_HairPrestationCollectionItem_NominativeServiceCommand_Query~",
                table: "HairPrestationCollectionItem");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommand_Activities_ActivityCode",
                table: "NominativeServiceCommand");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommand_AspNetUsers_ClientId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommand_BrusherProfile_SelectedProfileUser~",
                table: "NominativeServiceCommand");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommand_GitRepositoryReference_GitId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommand_HairPrestation_PrestationId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommand_Locations_HairMultiCutQuery_Locati~",
                table: "NominativeServiceCommand");


            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommand_Locations_RdvQuery_LocationId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommand_PayPalPayment_PaymentId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommand_Performers_PerformerId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectBuildConfiguration_NominativeServiceCommand_ProjectId",
                table: "ProjectBuildConfiguration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NominativeServiceCommand",
                table: "NominativeServiceCommand");

            migrationBuilder.RenameTable(
                name: "NominativeServiceCommand",
                newName: "NominativeServiceCommands");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommand_SelectedProfileUserId",
                table: "NominativeServiceCommands",
                newName: "IX_NominativeServiceCommands_SelectedProfileUserId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommand_RdvQuery_LocationId",
                table: "NominativeServiceCommands",
                newName: "IX_NominativeServiceCommands_RdvQuery_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommand_PrestationId",
                table: "NominativeServiceCommands",
                newName: "IX_NominativeServiceCommands_PrestationId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommand_PerformerId",
                table: "NominativeServiceCommands",
                newName: "IX_NominativeServiceCommands_PerformerId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommand_PaymentId",
                table: "NominativeServiceCommands",
                newName: "IX_NominativeServiceCommands_PaymentId");


            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommand_HairMultiCutQuery_LocationId",
                table: "NominativeServiceCommands",
                newName: "IX_NominativeServiceCommands_HairMultiCutQuery_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommand_GitId",
                table: "NominativeServiceCommands",
                newName: "IX_NominativeServiceCommands_GitId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommand_ClientId",
                table: "NominativeServiceCommands",
                newName: "IX_NominativeServiceCommands_ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommand_ActivityCode",
                table: "NominativeServiceCommands",
                newName: "IX_NominativeServiceCommands_ActivityCode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NominativeServiceCommands",
                table: "NominativeServiceCommands",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Estimates_NominativeServiceCommands_CommandId",
                table: "Estimates",
                column: "CommandId",
                principalTable: "NominativeServiceCommands",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HairPrestationCollectionItem_NominativeServiceCommands_Quer~",
                table: "HairPrestationCollectionItem",
                column: "QueryId",
                principalTable: "NominativeServiceCommands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommands_Activities_ActivityCode",
                table: "NominativeServiceCommands",
                column: "ActivityCode",
                principalTable: "Activities",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommands_AspNetUsers_ClientId",
                table: "NominativeServiceCommands",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommands_BrusherProfile_SelectedProfileUse~",
                table: "NominativeServiceCommands",
                column: "SelectedProfileUserId",
                principalTable: "BrusherProfile",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommands_GitRepositoryReference_GitId",
                table: "NominativeServiceCommands",
                column: "GitId",
                principalTable: "GitRepositoryReference",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommands_HairPrestation_PrestationId",
                table: "NominativeServiceCommands",
                column: "PrestationId",
                principalTable: "HairPrestation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommands_Locations_HairMultiCutQuery_Locat~",
                table: "NominativeServiceCommands",
                column: "HairMultiCutQuery_LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommands_Locations_RdvQuery_LocationId",
                table: "NominativeServiceCommands",
                column: "RdvQuery_LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommands_PayPalPayment_PaymentId",
                table: "NominativeServiceCommands",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken");

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommands_Performers_PerformerId",
                table: "NominativeServiceCommands",
                column: "PerformerId",
                principalTable: "Performers",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectBuildConfiguration_NominativeServiceCommands_Project~",
                table: "ProjectBuildConfiguration",
                column: "ProjectId",
                principalTable: "NominativeServiceCommands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estimates_NominativeServiceCommands_CommandId",
                table: "Estimates");

            migrationBuilder.DropForeignKey(
                name: "FK_HairPrestationCollectionItem_NominativeServiceCommands_Quer~",
                table: "HairPrestationCollectionItem");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommands_Activities_ActivityCode",
                table: "NominativeServiceCommands");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommands_AspNetUsers_ClientId",
                table: "NominativeServiceCommands");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommands_BrusherProfile_SelectedProfileUse~",
                table: "NominativeServiceCommands");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommands_GitRepositoryReference_GitId",
                table: "NominativeServiceCommands");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommands_HairPrestation_PrestationId",
                table: "NominativeServiceCommands");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommands_Locations_HairMultiCutQuery_Locat~",
                table: "NominativeServiceCommands");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommands_Locations_LocationId",
                table: "NominativeServiceCommands");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommands_Locations_RdvQuery_LocationId",
                table: "NominativeServiceCommands");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommands_PayPalPayment_PaymentId",
                table: "NominativeServiceCommands");

            migrationBuilder.DropForeignKey(
                name: "FK_NominativeServiceCommands_Performers_PerformerId",
                table: "NominativeServiceCommands");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectBuildConfiguration_NominativeServiceCommands_Project~",
                table: "ProjectBuildConfiguration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NominativeServiceCommands",
                table: "NominativeServiceCommands");

            migrationBuilder.RenameTable(
                name: "NominativeServiceCommands",
                newName: "NominativeServiceCommand");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommands_SelectedProfileUserId",
                table: "NominativeServiceCommand",
                newName: "IX_NominativeServiceCommand_SelectedProfileUserId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommands_RdvQuery_LocationId",
                table: "NominativeServiceCommand",
                newName: "IX_NominativeServiceCommand_RdvQuery_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommands_PrestationId",
                table: "NominativeServiceCommand",
                newName: "IX_NominativeServiceCommand_PrestationId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommands_PerformerId",
                table: "NominativeServiceCommand",
                newName: "IX_NominativeServiceCommand_PerformerId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommands_PaymentId",
                table: "NominativeServiceCommand",
                newName: "IX_NominativeServiceCommand_PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommands_HairMultiCutQuery_LocationId",
                table: "NominativeServiceCommand",
                newName: "IX_NominativeServiceCommand_HairMultiCutQuery_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommands_GitId",
                table: "NominativeServiceCommand",
                newName: "IX_NominativeServiceCommand_GitId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommands_ClientId",
                table: "NominativeServiceCommand",
                newName: "IX_NominativeServiceCommand_ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommands_ActivityCode",
                table: "NominativeServiceCommand",
                newName: "IX_NominativeServiceCommand_ActivityCode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NominativeServiceCommand",
                table: "NominativeServiceCommand",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Estimates_NominativeServiceCommand_CommandId",
                table: "Estimates",
                column: "CommandId",
                principalTable: "NominativeServiceCommand",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HairPrestationCollectionItem_NominativeServiceCommand_Query~",
                table: "HairPrestationCollectionItem",
                column: "QueryId",
                principalTable: "NominativeServiceCommand",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommand_Activities_ActivityCode",
                table: "NominativeServiceCommand",
                column: "ActivityCode",
                principalTable: "Activities",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommand_AspNetUsers_ClientId",
                table: "NominativeServiceCommand",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommand_BrusherProfile_SelectedProfileUser~",
                table: "NominativeServiceCommand",
                column: "SelectedProfileUserId",
                principalTable: "BrusherProfile",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommand_GitRepositoryReference_GitId",
                table: "NominativeServiceCommand",
                column: "GitId",
                principalTable: "GitRepositoryReference",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommand_HairPrestation_PrestationId",
                table: "NominativeServiceCommand",
                column: "PrestationId",
                principalTable: "HairPrestation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommand_Locations_HairMultiCutQuery_Locati~",
                table: "NominativeServiceCommand",
                column: "HairMultiCutQuery_LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);


            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommand_Locations_RdvQuery_LocationId",
                table: "NominativeServiceCommand",
                column: "RdvQuery_LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommand_PayPalPayment_PaymentId",
                table: "NominativeServiceCommand",
                column: "PaymentId",
                principalTable: "PayPalPayment",
                principalColumn: "CreationToken");

            migrationBuilder.AddForeignKey(
                name: "FK_NominativeServiceCommand_Performers_PerformerId",
                table: "NominativeServiceCommand",
                column: "PerformerId",
                principalTable: "Performers",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectBuildConfiguration_NominativeServiceCommand_ProjectId",
                table: "ProjectBuildConfiguration",
                column: "ProjectId",
                principalTable: "NominativeServiceCommand",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
