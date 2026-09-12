using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class genericEstimate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estimates_RdvQueries_CommandId",
                table: "Estimates");

            migrationBuilder.DropForeignKey(
                name: "FK_HairPrestationCollectionItem_HairMultiCutQueries_QueryId",
                table: "HairPrestationCollectionItem");

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

            migrationBuilder.DropTable(
                name: "HairCutQueries");

            migrationBuilder.DropTable(
                name: "HairMultiCutQueries");

            migrationBuilder.DropTable(
                name: "Project");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RdvQueries",
                table: "RdvQueries");

            migrationBuilder.RenameTable(
                name: "RdvQueries",
                newName: "NominativeServiceCommand");

            migrationBuilder.RenameIndex(
                name: "IX_RdvQueries_PerformerId",
                table: "NominativeServiceCommand",
                newName: "IX_NominativeServiceCommand_PerformerId");

            migrationBuilder.RenameIndex(
                name: "IX_RdvQueries_PaymentId",
                table: "NominativeServiceCommand",
                newName: "IX_NominativeServiceCommand_PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_RdvQueries_LocationId",
                table: "NominativeServiceCommand",
                newName: "IX_NominativeServiceCommand_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_RdvQueries_ClientId",
                table: "NominativeServiceCommand",
                newName: "IX_NominativeServiceCommand_ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_RdvQueries_ActivityCode",
                table: "NominativeServiceCommand",
                newName: "IX_NominativeServiceCommand_ActivityCode");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "NominativeServiceCommand",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "LocationType",
                table: "NominativeServiceCommand",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "LocationId",
                table: "NominativeServiceCommand",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EventDate",
                table: "NominativeServiceCommand",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalInfo",
                table: "NominativeServiceCommand",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "NominativeServiceCommand",
                type: "character varying(34)",
                maxLength: 34,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "GitId",
                table: "NominativeServiceCommand",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HairMultiCutQuery_EventDate",
                table: "NominativeServiceCommand",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "HairMultiCutQuery_LocationId",
                table: "NominativeServiceCommand",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "NominativeServiceCommand",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "NominativeServiceCommand",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PrestationId",
                table: "NominativeServiceCommand",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RdvQuery_EventDate",
                table: "NominativeServiceCommand",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RdvQuery_LocationId",
                table: "NominativeServiceCommand",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedProfileUserId",
                table: "NominativeServiceCommand",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "NominativeServiceCommand",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_NominativeServiceCommand",
                table: "NominativeServiceCommand",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DictionnaireMetier",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Langue = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DomaineActiviteCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictionnaireMetier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DictionnaireMetier_Activities_DomaineActiviteCode",
                        column: x => x.DomaineActiviteCode,
                        principalTable: "Activities",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TermeMetier",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DictionnaireMetierId = table.Column<long>(type: "bigint", nullable: false),
                    Mot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Definition = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Langue = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StatutValidation = table.Column<int>(type: "integer", nullable: false),
                    ProposeParId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ValideParId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    DateSoumission = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateValidation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermeMetier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TermeMetier_AspNetUsers_ProposeParId",
                        column: x => x.ProposeParId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TermeMetier_AspNetUsers_ValideParId",
                        column: x => x.ValideParId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TermeMetier_DictionnaireMetier_DictionnaireMetierId",
                        column: x => x.DictionnaireMetierId,
                        principalTable: "DictionnaireMetier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NominativeServiceCommand_GitId",
                table: "NominativeServiceCommand",
                column: "GitId");

            migrationBuilder.CreateIndex(
                name: "IX_NominativeServiceCommand_HairMultiCutQuery_LocationId",
                table: "NominativeServiceCommand",
                column: "HairMultiCutQuery_LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_NominativeServiceCommand_PrestationId",
                table: "NominativeServiceCommand",
                column: "PrestationId");

            migrationBuilder.CreateIndex(
                name: "IX_NominativeServiceCommand_RdvQuery_LocationId",
                table: "NominativeServiceCommand",
                column: "RdvQuery_LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_NominativeServiceCommand_SelectedProfileUserId",
                table: "NominativeServiceCommand",
                column: "SelectedProfileUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DictionnaireMetier_DomaineActiviteCode_Langue_Nom",
                table: "DictionnaireMetier",
                columns: new[] { "DomaineActiviteCode", "Langue", "Nom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TermeMetier_DictionnaireMetierId_Langue_Mot",
                table: "TermeMetier",
                columns: new[] { "DictionnaireMetierId", "Langue", "Mot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TermeMetier_ProposeParId",
                table: "TermeMetier",
                column: "ProposeParId");

            migrationBuilder.CreateIndex(
                name: "IX_TermeMetier_ValideParId",
                table: "TermeMetier",
                column: "ValideParId");

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
                name: "FK_NominativeServiceCommand_Locations_LocationId",
                table: "NominativeServiceCommand",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "FK_NominativeServiceCommand_Locations_LocationId",
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

            migrationBuilder.DropTable(
                name: "TermeMetier");

            migrationBuilder.DropTable(
                name: "DictionnaireMetier");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NominativeServiceCommand",
                table: "NominativeServiceCommand");

            migrationBuilder.DropIndex(
                name: "IX_NominativeServiceCommand_GitId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropIndex(
                name: "IX_NominativeServiceCommand_HairMultiCutQuery_LocationId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropIndex(
                name: "IX_NominativeServiceCommand_PrestationId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropIndex(
                name: "IX_NominativeServiceCommand_RdvQuery_LocationId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropIndex(
                name: "IX_NominativeServiceCommand_SelectedProfileUserId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropColumn(
                name: "AdditionalInfo",
                table: "NominativeServiceCommand");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "NominativeServiceCommand");

            migrationBuilder.DropColumn(
                name: "GitId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropColumn(
                name: "HairMultiCutQuery_EventDate",
                table: "NominativeServiceCommand");

            migrationBuilder.DropColumn(
                name: "HairMultiCutQuery_LocationId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "NominativeServiceCommand");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropColumn(
                name: "PrestationId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropColumn(
                name: "RdvQuery_EventDate",
                table: "NominativeServiceCommand");

            migrationBuilder.DropColumn(
                name: "RdvQuery_LocationId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropColumn(
                name: "SelectedProfileUserId",
                table: "NominativeServiceCommand");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "NominativeServiceCommand");

            migrationBuilder.RenameTable(
                name: "NominativeServiceCommand",
                newName: "RdvQueries");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommand_PerformerId",
                table: "RdvQueries",
                newName: "IX_RdvQueries_PerformerId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommand_PaymentId",
                table: "RdvQueries",
                newName: "IX_RdvQueries_PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommand_LocationId",
                table: "RdvQueries",
                newName: "IX_RdvQueries_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommand_ClientId",
                table: "RdvQueries",
                newName: "IX_RdvQueries_ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_NominativeServiceCommand_ActivityCode",
                table: "RdvQueries",
                newName: "IX_RdvQueries_ActivityCode");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "RdvQueries",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LocationType",
                table: "RdvQueries",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
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

            migrationBuilder.AlterColumn<DateTime>(
                name: "EventDate",
                table: "RdvQueries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RdvQueries",
                table: "RdvQueries",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "HairCutQueries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActivityCode = table.Column<string>(type: "text", nullable: false),
                    ClientId = table.Column<string>(type: "text", nullable: false),
                    LocationId = table.Column<long>(type: "bigint", nullable: true),
                    PaymentId = table.Column<string>(type: "text", nullable: true),
                    PerformerId = table.Column<string>(type: "text", nullable: false),
                    PrestationId = table.Column<long>(type: "bigint", nullable: false),
                    SelectedProfileUserId = table.Column<string>(type: "text", nullable: true),
                    AdditionalInfo = table.Column<string>(type: "text", nullable: false),
                    Consent = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Provisional = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: false),
                    UserModified = table.Column<string>(type: "text", nullable: false),
                    ValidationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HairCutQueries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HairCutQueries_Activities_ActivityCode",
                        column: x => x.ActivityCode,
                        principalTable: "Activities",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairCutQueries_AspNetUsers_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairCutQueries_BrusherProfile_SelectedProfileUserId",
                        column: x => x.SelectedProfileUserId,
                        principalTable: "BrusherProfile",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_HairCutQueries_HairPrestation_PrestationId",
                        column: x => x.PrestationId,
                        principalTable: "HairPrestation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairCutQueries_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HairCutQueries_PayPalPayment_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "PayPalPayment",
                        principalColumn: "CreationToken");
                    table.ForeignKey(
                        name: "FK_HairCutQueries_Performers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "Performers",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HairMultiCutQueries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActivityCode = table.Column<string>(type: "text", nullable: false),
                    ClientId = table.Column<string>(type: "text", nullable: false),
                    LocationId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentId = table.Column<string>(type: "text", nullable: true),
                    PerformerId = table.Column<string>(type: "text", nullable: false),
                    Consent = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Provisional = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: false),
                    UserModified = table.Column<string>(type: "text", nullable: false),
                    ValidationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HairMultiCutQueries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HairMultiCutQueries_Activities_ActivityCode",
                        column: x => x.ActivityCode,
                        principalTable: "Activities",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairMultiCutQueries_AspNetUsers_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairMultiCutQueries_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HairMultiCutQueries_PayPalPayment_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "PayPalPayment",
                        principalColumn: "CreationToken");
                    table.ForeignKey(
                        name: "FK_HairMultiCutQueries_Performers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "Performers",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActivityCode = table.Column<string>(type: "text", nullable: false),
                    ClientId = table.Column<string>(type: "text", nullable: false),
                    GitId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentId = table.Column<string>(type: "text", nullable: true),
                    PerformerId = table.Column<string>(type: "text", nullable: false),
                    Consent = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: true),
                    Provisional = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: false),
                    UserModified = table.Column<string>(type: "text", nullable: false),
                    ValidationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_Activities_ActivityCode",
                        column: x => x.ActivityCode,
                        principalTable: "Activities",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Project_AspNetUsers_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Project_GitRepositoryReference_GitId",
                        column: x => x.GitId,
                        principalTable: "GitRepositoryReference",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Project_PayPalPayment_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "PayPalPayment",
                        principalColumn: "CreationToken");
                    table.ForeignKey(
                        name: "FK_Project_Performers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "Performers",
                        principalColumn: "PerformerId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.AddForeignKey(
                name: "FK_Estimates_RdvQueries_CommandId",
                table: "Estimates",
                column: "CommandId",
                principalTable: "RdvQueries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HairPrestationCollectionItem_HairMultiCutQueries_QueryId",
                table: "HairPrestationCollectionItem",
                column: "QueryId",
                principalTable: "HairMultiCutQueries",
                principalColumn: "Id",
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
                principalColumn: "CreationToken");

            migrationBuilder.AddForeignKey(
                name: "FK_RdvQueries_Performers_PerformerId",
                table: "RdvQueries",
                column: "PerformerId",
                principalTable: "Performers",
                principalColumn: "PerformerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
