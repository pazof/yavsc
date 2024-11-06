using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yavsc.Migrations
{
    /// <inheritdoc />
    public partial class dismiss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bug_Feature_FeatureId",
                table: "Bug");

            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQueries_Locations_LocationId",
                table: "HairCutQueries");

            migrationBuilder.DropTable(
                name: "DimissClicked");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "RdvQueries");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "HairMultiCutQueries");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "HairCutQueries");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DateModified",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserCreated",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserModified",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Rejected",
                table: "RdvQueries",
                newName: "Decided");

            migrationBuilder.RenameColumn(
                name: "Rejected",
                table: "Project",
                newName: "Decided");

            migrationBuilder.RenameColumn(
                name: "Rejected",
                table: "HairMultiCutQueries",
                newName: "Decided");

            migrationBuilder.RenameColumn(
                name: "Rejected",
                table: "HairCutQueries",
                newName: "Decided");

            migrationBuilder.AddColumn<bool>(
                name: "Accepted",
                table: "RdvQueries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Accepted",
                table: "Project",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Accepted",
                table: "HairMultiCutQueries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<long>(
                name: "LocationId",
                table: "HairCutQueries",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<bool>(
                name: "Accepted",
                table: "HairCutQueries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<long>(
                name: "FeatureId",
                table: "Bug",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Secret",
                table: "Applications",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "RedirectUri",
                table: "Applications",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LogoutRedirectUri",
                table: "Applications",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "Applications",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "AccessTokenLifetime",
                table: "Applications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DismissClicked",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    NotificationId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DismissClicked", x => new { x.UserId, x.NotificationId });
                    table.ForeignKey(
                        name: "FK_DismissClicked_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DismissClicked_Notification_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scopes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scopes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DismissClicked_NotificationId",
                table: "DismissClicked",
                column: "NotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bug_Feature_FeatureId",
                table: "Bug",
                column: "FeatureId",
                principalTable: "Feature",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQueries_Locations_LocationId",
                table: "HairCutQueries",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bug_Feature_FeatureId",
                table: "Bug");

            migrationBuilder.DropForeignKey(
                name: "FK_HairCutQueries_Locations_LocationId",
                table: "HairCutQueries");

            migrationBuilder.DropTable(
                name: "DismissClicked");

            migrationBuilder.DropTable(
                name: "Scopes");

            migrationBuilder.DropColumn(
                name: "Accepted",
                table: "RdvQueries");

            migrationBuilder.DropColumn(
                name: "Accepted",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "Accepted",
                table: "HairMultiCutQueries");

            migrationBuilder.DropColumn(
                name: "Accepted",
                table: "HairCutQueries");

            migrationBuilder.DropColumn(
                name: "AccessTokenLifetime",
                table: "Applications");

            migrationBuilder.RenameColumn(
                name: "Decided",
                table: "RdvQueries",
                newName: "Rejected");

            migrationBuilder.RenameColumn(
                name: "Decided",
                table: "Project",
                newName: "Rejected");

            migrationBuilder.RenameColumn(
                name: "Decided",
                table: "HairMultiCutQueries",
                newName: "Rejected");

            migrationBuilder.RenameColumn(
                name: "Decided",
                table: "HairCutQueries",
                newName: "Rejected");

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "RdvQueries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "Project",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "HairMultiCutQueries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<long>(
                name: "LocationId",
                table: "HairCutQueries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "HairCutQueries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<long>(
                name: "FeatureId",
                table: "Bug",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModified",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UserCreated",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserModified",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Secret",
                table: "Applications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "RedirectUri",
                table: "Applications",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LogoutRedirectUri",
                table: "Applications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "Applications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.CreateTable(
                name: "DimissClicked",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    NotificationId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DimissClicked", x => new { x.UserId, x.NotificationId });
                    table.ForeignKey(
                        name: "FK_DimissClicked_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DimissClicked_Notification_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DimissClicked_NotificationId",
                table: "DimissClicked",
                column: "NotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bug_Feature_FeatureId",
                table: "Bug",
                column: "FeatureId",
                principalTable: "Feature",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HairCutQueries_Locations_LocationId",
                table: "HairCutQueries",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
