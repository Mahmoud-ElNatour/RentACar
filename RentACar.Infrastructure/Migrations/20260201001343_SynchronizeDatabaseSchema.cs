using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SynchronizeDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropTable(
            //     name: "CustomerCreditCard");

            // migrationBuilder.DropTable(
            //     name: "CreditCard");

            // migrationBuilder.DropColumn(
            //     name: "creditcardID",
            //     table: "Payments");

            // migrationBuilder.RenameIndex(
            //     name: "IX_Payments_bookingID",
            //     table: "Payments",
            //     newName: "IX_Payments_BookingId");

            // migrationBuilder.RenameIndex(
            //     name: "IX_Customers_aspNetUserId",
            //     table: "Customers",
            //     newName: "IX_Customers_AspNetUserId");

            // migrationBuilder.RenameIndex(
            //     name: "IX_Bookings_customerID",
            //     table: "Bookings",
            //     newName: "IX_Bookings_CustomerId");

            // migrationBuilder.AddColumn<bool>(
            //     name: "isExpiredNotificationSent",
            //     table: "Promocodes",
            //     type: "bit",
            //     nullable: false,
            //     defaultValue: false);

            // migrationBuilder.AddColumn<DateTime>(
            //     name: "lastPaymentReminderSentAt",
            //     table: "Bookings",
            //     type: "datetime2",
            //     nullable: true);

            // migrationBuilder.AddColumn<DateTime>(
            //     name: "lastPickupReminderSentAt",
            //     table: "Bookings",
            //     type: "datetime2",
            //     nullable: true);

            // migrationBuilder.AddColumn<DateTime>(
            //     name: "lastReturnReminderSentAt",
            //     table: "Bookings",
            //     type: "datetime2",
            //     nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "driverDailyFee",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "driverID",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "hasDriver",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "pickupAddress",
                table: "Bookings",
                type: "nvarchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "pickupDateTime",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            // migrationBuilder.AddColumn<string>(
            //     name: "UserId",
            //     table: "AuditLogs",
            //     type: "nvarchar(450)",
            //     nullable: true);

            // migrationBuilder.CreateTable(
            //     name: "CustomerRatings",
            //     columns: table => new
            //     {
            //         customerRatingID = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         customerID = table.Column<int>(type: "int", nullable: false),
            //         bookingID = table.Column<int>(type: "int", nullable: false),
            //         stars = table.Column<int>(type: "int", nullable: false),
            //         feedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         ratingDate = table.Column<DateTime>(type: "datetime2", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_CustomerRatings", x => x.customerRatingID);
            //         table.ForeignKey(
            //             name: "FK_CustomerRatings_Bookings_bookingID",
            //             column: x => x.bookingID,
            //             principalTable: "Bookings",
            //             principalColumn: "BookingID",
            //             onDelete: ReferentialAction.Cascade);
            //         table.ForeignKey(
            //             name: "FK_CustomerRatings_Customers_customerID",
            //             column: x => x.customerID,
            //             principalTable: "Customers",
            //             principalColumn: "userID",
            //             onDelete: ReferentialAction.Cascade);
            //     });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "driverDailyFee",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "driverID",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "hasDriver",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "pickupAddress",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "pickupDateTime",
                table: "Bookings");
        }
    }
}
