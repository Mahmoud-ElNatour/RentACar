using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceEmployeeIdWithBookingId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerRatings_Employees_employeeID",
                table: "CustomerRatings");

            migrationBuilder.DropIndex(
                name: "IX_CustomerRatings_employeeID",
                table: "CustomerRatings");

            migrationBuilder.DropColumn(
                name: "employeeID",
                table: "CustomerRatings");

            migrationBuilder.AddColumn<int>(
                name: "bookingID",
                table: "CustomerRatings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRatings_bookingID",
                table: "CustomerRatings",
                column: "bookingID");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerRatings_Bookings_bookingID",
                table: "CustomerRatings",
                column: "bookingID",
                principalTable: "Bookings",
                principalColumn: "BookingID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerRatings_Bookings_bookingID",
                table: "CustomerRatings");

            migrationBuilder.DropIndex(
                name: "IX_CustomerRatings_bookingID",
                table: "CustomerRatings");

            migrationBuilder.DropColumn(
                name: "bookingID",
                table: "CustomerRatings");

            migrationBuilder.AddColumn<int>(
                name: "employeeID",
                table: "CustomerRatings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRatings_employeeID",
                table: "CustomerRatings",
                column: "employeeID");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerRatings_Employees_employeeID",
                table: "CustomerRatings",
                column: "employeeID",
                principalTable: "Employees",
                principalColumn: "employeeID");
        }
    }
}
