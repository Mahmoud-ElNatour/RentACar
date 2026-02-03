using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverAvailabilityAdvanced : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DriverAvailabilities_driverID",
                table: "DriverAvailabilities");

            migrationBuilder.AlterColumn<DateTime>(
                name: "startDateTime",
                table: "DriverAvailabilities",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "isRecurringWeekly",
                table: "DriverAvailabilities",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "endDateTime",
                table: "DriverAvailabilities",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateOnly>(
                name: "date",
                table: "DriverAvailabilities",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "endTime",
                table: "DriverAvailabilities",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "startTime",
                table: "DriverAvailabilities",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updatedAt",
                table: "DriverAvailabilities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_DriverAvailabilities_driverID_date",
                table: "DriverAvailabilities",
                columns: new[] { "driverID", "date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DriverAvailabilities_driverID_date",
                table: "DriverAvailabilities");

            migrationBuilder.DropColumn(
                name: "date",
                table: "DriverAvailabilities");

            migrationBuilder.DropColumn(
                name: "endTime",
                table: "DriverAvailabilities");

            migrationBuilder.DropColumn(
                name: "startTime",
                table: "DriverAvailabilities");

            migrationBuilder.DropColumn(
                name: "updatedAt",
                table: "DriverAvailabilities");

            migrationBuilder.AlterColumn<DateTime>(
                name: "startDateTime",
                table: "DriverAvailabilities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isRecurringWeekly",
                table: "DriverAvailabilities",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "endDateTime",
                table: "DriverAvailabilities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DriverAvailabilities_driverID",
                table: "DriverAvailabilities",
                column: "driverID");
        }
    }
}
