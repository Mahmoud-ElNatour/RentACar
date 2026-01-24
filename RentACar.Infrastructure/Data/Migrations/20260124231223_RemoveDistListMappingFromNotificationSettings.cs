using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDistListMappingFromNotificationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarUpdateEmployeesListId",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "CategoryUpdateEmployeesListId",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "DocsReminderEmployeesListId",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "EmployeesDefaultListId",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "PaymentReminderMaxSends",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "PaymentReminderRepeatEveryHours",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "PaymentReminderSendOnceOnly",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "PickupReminderSendOnceOnly",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "PromoExpiryEmployeesListId",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "PromocodeUpdateEmployeesListId",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "ReturnReminderSendOnceOnly",
                table: "NotificationSettings");

            migrationBuilder.RenameColumn(
                name: "ReturnReminderHoursBefore",
                table: "NotificationSettings",
                newName: "PromoExpiryDaysBefore");

            migrationBuilder.RenameColumn(
                name: "PickupReminderHoursBefore",
                table: "NotificationSettings",
                newName: "PaymentReminderMaxDurationHours");

            migrationBuilder.RenameColumn(
                name: "PaymentReminderDelayHours",
                table: "NotificationSettings",
                newName: "PaymentReminderIntervalHours");

            migrationBuilder.AddColumn<int>(
                name: "PaymentReminderInitialDelayHours",
                table: "NotificationSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PickupReminderScheduleHoursCsv",
                table: "NotificationSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReturnReminderScheduleHoursCsv",
                table: "NotificationSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentReminderInitialDelayHours",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "PickupReminderScheduleHoursCsv",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "ReturnReminderScheduleHoursCsv",
                table: "NotificationSettings");

            migrationBuilder.RenameColumn(
                name: "PromoExpiryDaysBefore",
                table: "NotificationSettings",
                newName: "ReturnReminderHoursBefore");

            migrationBuilder.RenameColumn(
                name: "PaymentReminderMaxDurationHours",
                table: "NotificationSettings",
                newName: "PickupReminderHoursBefore");

            migrationBuilder.RenameColumn(
                name: "PaymentReminderIntervalHours",
                table: "NotificationSettings",
                newName: "PaymentReminderDelayHours");

            migrationBuilder.AddColumn<int>(
                name: "CarUpdateEmployeesListId",
                table: "NotificationSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoryUpdateEmployeesListId",
                table: "NotificationSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocsReminderEmployeesListId",
                table: "NotificationSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeesDefaultListId",
                table: "NotificationSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentReminderMaxSends",
                table: "NotificationSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentReminderRepeatEveryHours",
                table: "NotificationSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PaymentReminderSendOnceOnly",
                table: "NotificationSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PickupReminderSendOnceOnly",
                table: "NotificationSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PromoExpiryEmployeesListId",
                table: "NotificationSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PromocodeUpdateEmployeesListId",
                table: "NotificationSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReturnReminderSendOnceOnly",
                table: "NotificationSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
