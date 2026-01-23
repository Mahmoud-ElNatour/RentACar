using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EmailConfigCenterEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocsReminderEmployeesListId",
                table: "NotificationSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRunAt",
                table: "NotificationSettings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastRunFailedCount",
                table: "NotificationSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastRunProcessedCount",
                table: "NotificationSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastRunSentCount",
                table: "NotificationSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LastRunSummary",
                table: "NotificationSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextRunAt",
                table: "NotificationSettings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PaymentReminderSendOnceOnly",
                table: "NotificationSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReminderStatusCsv",
                table: "NotificationSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "PickupReminderSendOnceOnly",
                table: "NotificationSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReminderProcessingPaused",
                table: "NotificationSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReturnReminderSendOnceOnly",
                table: "NotificationSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "NotificationSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "NotificationSettings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSettings_UpdatedByUserId",
                table: "NotificationSettings",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationSettings_AspNetUsers_UpdatedByUserId",
                table: "NotificationSettings",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationSettings_AspNetUsers_UpdatedByUserId",
                table: "NotificationSettings");

            migrationBuilder.DropIndex(
                name: "IX_NotificationSettings_UpdatedByUserId",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "DocsReminderEmployeesListId",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "LastRunAt",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "LastRunFailedCount",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "LastRunProcessedCount",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "LastRunSentCount",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "LastRunSummary",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "NextRunAt",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "PaymentReminderSendOnceOnly",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "PaymentReminderStatusCsv",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "PickupReminderSendOnceOnly",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "ReminderProcessingPaused",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "ReturnReminderSendOnceOnly",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "NotificationSettings");
        }
    }
}
