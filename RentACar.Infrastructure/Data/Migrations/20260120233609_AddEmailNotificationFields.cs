using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailNotificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isExpiredNotificationSent",
                table: "Promocodes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "lastPaymentReminderSentAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "lastPickupReminderSentAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "lastReturnReminderSentAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isExpiredNotificationSent",
                table: "Promocodes");

            migrationBuilder.DropColumn(
                name: "lastPaymentReminderSentAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "lastPickupReminderSentAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "lastReturnReminderSentAt",
                table: "Bookings");
        }
    }
}
