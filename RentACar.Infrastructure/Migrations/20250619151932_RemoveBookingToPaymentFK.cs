using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBookingToPaymentFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Payments",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Payments_bookingID",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_paymentID",
                table: "Bookings");

            migrationBuilder.AlterColumn<int>(
                name: "paymentID",
                table: "Bookings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_bookingID",
                table: "Payments",
                column: "bookingID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_bookingID",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "paymentID",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_bookingID",
                table: "Payments",
                column: "bookingID");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_paymentID",
                table: "Bookings",
                column: "paymentID");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Payments",
                table: "Bookings",
                column: "paymentID",
                principalTable: "Payments",
                principalColumn: "paymentID");
        }
    }
}
