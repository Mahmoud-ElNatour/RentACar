using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ApplyUserForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UpdatedByUserId",
                table: "EmailTemplates",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "EmailLogs",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "EmailDrafts",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedByUserId",
                table: "DistributionLists",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "DistributionLists",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AddedByUserId",
                table: "DistributionListMembers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "AuditLogs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_UpdatedByUserId",
                table: "EmailTemplates",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_CreatedByUserId",
                table: "EmailLogs",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailDrafts_CreatedByUserId",
                table: "EmailDrafts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionLists_CreatedByUserId",
                table: "DistributionLists",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionLists_UpdatedByUserId",
                table: "DistributionLists",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionListMembers_AddedByUserId",
                table: "DistributionListMembers",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_AspNetUsers_UserId",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DistributionListMembers_AspNetUsers_AddedByUserId",
                table: "DistributionListMembers",
                column: "AddedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DistributionLists_AspNetUsers_CreatedByUserId",
                table: "DistributionLists",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DistributionLists_AspNetUsers_UpdatedByUserId",
                table: "DistributionLists",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailDrafts_AspNetUsers_CreatedByUserId",
                table: "EmailDrafts",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailLogs_AspNetUsers_CreatedByUserId",
                table: "EmailLogs",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailTemplates_AspNetUsers_UpdatedByUserId",
                table: "EmailTemplates",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_AspNetUsers_UserId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_DistributionListMembers_AspNetUsers_AddedByUserId",
                table: "DistributionListMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_DistributionLists_AspNetUsers_CreatedByUserId",
                table: "DistributionLists");

            migrationBuilder.DropForeignKey(
                name: "FK_DistributionLists_AspNetUsers_UpdatedByUserId",
                table: "DistributionLists");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailDrafts_AspNetUsers_CreatedByUserId",
                table: "EmailDrafts");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailLogs_AspNetUsers_CreatedByUserId",
                table: "EmailLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailTemplates_AspNetUsers_UpdatedByUserId",
                table: "EmailTemplates");

            migrationBuilder.DropIndex(
                name: "IX_EmailTemplates_UpdatedByUserId",
                table: "EmailTemplates");

            migrationBuilder.DropIndex(
                name: "IX_EmailLogs_CreatedByUserId",
                table: "EmailLogs");

            migrationBuilder.DropIndex(
                name: "IX_EmailDrafts_CreatedByUserId",
                table: "EmailDrafts");

            migrationBuilder.DropIndex(
                name: "IX_DistributionLists_CreatedByUserId",
                table: "DistributionLists");

            migrationBuilder.DropIndex(
                name: "IX_DistributionLists_UpdatedByUserId",
                table: "DistributionLists");

            migrationBuilder.DropIndex(
                name: "IX_DistributionListMembers_AddedByUserId",
                table: "DistributionListMembers");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedByUserId",
                table: "EmailTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "EmailLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "EmailDrafts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedByUserId",
                table: "DistributionLists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "DistributionLists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AddedByUserId",
                table: "DistributionListMembers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
