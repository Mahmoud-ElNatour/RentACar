using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class addedDriver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "driverFee",
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
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "pickupLat",
                table: "Bookings",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "pickupLng",
                table: "Bookings",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    driverID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    aspNetUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    displayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    isAvailableManual = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.driverID);
                    table.ForeignKey(
                        name: "FK_Drivers_AspNetUsers",
                        column: x => x.aspNetUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DriverAvailabilities",
                columns: table => new
                {
                    driverAvailabilityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    driverID = table.Column<int>(type: "int", nullable: false),
                    startTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    endTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isRecurringWeekly = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverAvailabilities", x => x.driverAvailabilityID);
                    table.ForeignKey(
                        name: "FK_DriverAvailabilities_Drivers",
                        column: x => x.driverID,
                        principalTable: "Drivers",
                        principalColumn: "driverID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DriverLocations",
                columns: table => new
                {
                    driverLocationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    driverID = table.Column<int>(type: "int", nullable: false),
                    bookingID = table.Column<int>(type: "int", nullable: false),
                    latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    lastUpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    isTrackingActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverLocations", x => x.driverLocationID);
                    table.ForeignKey(
                        name: "FK_DriverLocations_Bookings",
                        column: x => x.bookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverLocations_Drivers",
                        column: x => x.driverID,
                        principalTable: "Drivers",
                        principalColumn: "driverID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_driverID",
                table: "Bookings",
                column: "driverID");

            migrationBuilder.CreateIndex(
                name: "IX_DriverAvailabilities_driverID",
                table: "DriverAvailabilities",
                column: "driverID");

            migrationBuilder.CreateIndex(
                name: "IX_DriverLocations_bookingID",
                table: "DriverLocations",
                column: "bookingID");

            migrationBuilder.CreateIndex(
                name: "IX_DriverLocations_driverID",
                table: "DriverLocations",
                column: "driverID");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_aspNetUserId",
                table: "Drivers",
                column: "aspNetUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Drivers",
                table: "Bookings",
                column: "driverID",
                principalTable: "Drivers",
                principalColumn: "driverID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Drivers",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "DriverAvailabilities");

            migrationBuilder.DropTable(
                name: "DriverLocations");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_driverID",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "driverFee",
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
                name: "pickupLat",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "pickupLng",
                table: "Bookings");
        }
    }
}
