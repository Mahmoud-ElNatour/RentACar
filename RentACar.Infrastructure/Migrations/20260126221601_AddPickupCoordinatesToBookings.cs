using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPickupCoordinatesToBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "pickupLatitude",
                table: "Bookings",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "pickupLongitude",
                table: "Bookings",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "pickupLatitude",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "pickupLongitude",
                table: "Bookings");
        }
    }
}
