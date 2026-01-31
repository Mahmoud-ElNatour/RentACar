using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    tripID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    bookingID = table.Column<int>(type: "int", nullable: false),
                    driverID = table.Column<int>(type: "int", nullable: true),
                    tripStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    startedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    arrivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    tripStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancelReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    lastDriverLatitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    lastDriverLongitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    lastLocationUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.tripID);
                    table.ForeignKey(
                        name: "FK_Trips_Bookings",
                        column: x => x.bookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trips_Drivers",
                        column: x => x.driverID,
                        principalTable: "Drivers",
                        principalColumn: "driverID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trips_bookingID",
                table: "Trips",
                column: "bookingID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_driverID",
                table: "Trips",
                column: "driverID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trips");
        }
    }
}
