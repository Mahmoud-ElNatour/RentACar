using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixMissingDriversTablesV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // -------------------------
            // DRIVERS
            // -------------------------
            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    driverID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    aspNetUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    employeeID = table.Column<int>(type: "int", nullable: false),

                    driverCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fullName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),

                    phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),

                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),

                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),

                    rating = table.Column<decimal>(type: "decimal(3,2)", nullable: true),

                    licenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    licenseExpiry = table.Column<DateOnly>(type: "date", nullable: true),

                    languages = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.driverID);

                    table.ForeignKey(
                        name: "FK_Drivers_AspNetUsers",
                        column: x => x.aspNetUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_Drivers_Employees",
                        column: x => x.employeeID,
                        principalTable: "Employees",
                        principalColumn: "employeeID",
                        onDelete: ReferentialAction.Restrict);
                });

            // -------------------------
            // DRIVER AVAILABILITY
            // -------------------------
            migrationBuilder.CreateTable(
                name: "DriverAvailabilities",
                columns: table => new
                {
                    driverAvailabilityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    driverID = table.Column<int>(type: "int", nullable: false),

                    startDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    endDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),

                    isRecurringWeekly = table.Column<bool>(type: "bit", nullable: false),
                    isAvailable = table.Column<bool>(type: "bit", nullable: false),

                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            // -------------------------
            // DRIVER LOCATION PINGS
            // -------------------------
            migrationBuilder.CreateTable(
                name: "DriverLocationPings",
                columns: table => new
                {
                    driverLocationPingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    bookingID = table.Column<int>(type: "int", nullable: false),
                    driverID = table.Column<int>(type: "int", nullable: false),

                    latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),

                    speed = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    heading = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    accuracyMeters = table.Column<decimal>(type: "decimal(10,2)", nullable: true),

                    batteryPercent = table.Column<int>(type: "int", nullable: true),

                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverLocationPings", x => x.driverLocationPingID);

                    table.ForeignKey(
                        name: "FK_DriverLocationPings_Drivers",
                        column: x => x.driverID,
                        principalTable: "Drivers",
                        principalColumn: "driverID",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_DriverLocationPings_Bookings",
                        column: x => x.bookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.Cascade);
                });

            // -------------------------
            // INDEXES
            // -------------------------
            migrationBuilder.CreateIndex(
                name: "IX_Drivers_aspNetUserId",
                table: "Drivers",
                column: "aspNetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_employeeID",
                table: "Drivers",
                column: "employeeID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DriverAvailabilities_driverID",
                table: "DriverAvailabilities",
                column: "driverID");

            migrationBuilder.CreateIndex(
                name: "IX_DriverLocationPings_driverID",
                table: "DriverLocationPings",
                column: "driverID");

            migrationBuilder.CreateIndex(
                name: "IX_DriverLocationPings_bookingID",
                table: "DriverLocationPings",
                column: "bookingID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DriverLocationPings");
            migrationBuilder.DropTable(name: "DriverAvailabilities");
            migrationBuilder.DropTable(name: "Drivers");
        }
    }
    }
