using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class UpdateIdentitySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerCreditCard");

            migrationBuilder.DropTable(
                name: "CreditCard");

            migrationBuilder.DropColumn(
                name: "creditcardID",
                table: "Payment");

            migrationBuilder.AddColumn<decimal>(
                name: "extraDriverFeePerDay",
                table: "Car",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "driverDailyFee",
                table: "Booking",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "driverID",
                table: "Booking",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "hasDriver",
                table: "Booking",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "pickupAddress",
                table: "Booking",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "pickupDateTime",
                table: "Booking",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "pickupLatitude",
                table: "Booking",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pickupLocationLabel",
                table: "Booking",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "pickupLongitude",
                table: "Booking",
                type: "float",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerRatings",
                columns: table => new
                {
                    customerRatingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customerID = table.Column<int>(type: "int", nullable: false),
                    bookingID = table.Column<int>(type: "int", nullable: false),
                    stars = table.Column<int>(type: "int", nullable: false),
                    feedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ratingDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerRatings", x => x.customerRatingID);
                    table.ForeignKey(
                        name: "FK_CustomerRatings_Booking_bookingID",
                        column: x => x.bookingID,
                        principalTable: "Booking",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerRatings_Customer_customerID",
                        column: x => x.customerID,
                        principalTable: "Customer",
                        principalColumn: "userID",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    isActive = table.Column<bool>(type: "bit", nullable: false),
                    dailyFeePerDay = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
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
                        name: "FK_Drivers_AspNetUser_aspNetUserId",
                        column: x => x.aspNetUserId,
                        principalTable: "AspNetUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Drivers_Employee_employeeID",
                        column: x => x.employeeID,
                        principalTable: "Employee",
                        principalColumn: "employeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DriverAllowedCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    driverID = table.Column<int>(type: "int", nullable: false),
                    categoryID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverAllowedCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverAllowedCategories_Category_categoryID",
                        column: x => x.categoryID,
                        principalTable: "Category",
                        principalColumn: "categoryID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverAllowedCategories_Drivers_driverID",
                        column: x => x.driverID,
                        principalTable: "Drivers",
                        principalColumn: "driverID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DriverAvailabilities",
                columns: table => new
                {
                    driverAvailabilityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    driverID = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    isAvailable = table.Column<bool>(type: "bit", nullable: false),
                    startTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    endTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    startDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    endDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    isRecurringWeekly = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverAvailabilities", x => x.driverAvailabilityID);
                    table.ForeignKey(
                        name: "FK_DriverAvailabilities_Drivers_driverID",
                        column: x => x.driverID,
                        principalTable: "Drivers",
                        principalColumn: "driverID",
                        onDelete: ReferentialAction.Cascade);
                });

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
                        name: "FK_DriverLocationPings_Booking_bookingID",
                        column: x => x.bookingID,
                        principalTable: "Booking",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverLocationPings_Drivers_driverID",
                        column: x => x.driverID,
                        principalTable: "Drivers",
                        principalColumn: "driverID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    tripID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    bookingID = table.Column<int>(type: "int", nullable: false),
                    driverID = table.Column<int>(type: "int", nullable: true),
                    tripStatus = table.Column<int>(type: "int", nullable: false),
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
                        name: "FK_Trips_Booking_bookingID",
                        column: x => x.bookingID,
                        principalTable: "Booking",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trips_Drivers_driverID",
                        column: x => x.driverID,
                        principalTable: "Drivers",
                        principalColumn: "driverID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Booking_driverID",
                table: "Booking",
                column: "driverID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRatings_bookingID",
                table: "CustomerRatings",
                column: "bookingID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRatings_customerID",
                table: "CustomerRatings",
                column: "customerID");

            migrationBuilder.CreateIndex(
                name: "IX_DriverAllowedCategories_categoryID",
                table: "DriverAllowedCategories",
                column: "categoryID");

            migrationBuilder.CreateIndex(
                name: "IX_DriverAllowedCategories_driverID",
                table: "DriverAllowedCategories",
                column: "driverID");

            migrationBuilder.CreateIndex(
                name: "IX_DriverAvailabilities_driverID",
                table: "DriverAvailabilities",
                column: "driverID");

            migrationBuilder.CreateIndex(
                name: "IX_DriverLocationPings_bookingID",
                table: "DriverLocationPings",
                column: "bookingID");

            migrationBuilder.CreateIndex(
                name: "IX_DriverLocationPings_driverID",
                table: "DriverLocationPings",
                column: "driverID");

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
                name: "IX_Trips_bookingID",
                table: "Trips",
                column: "bookingID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_driverID",
                table: "Trips",
                column: "driverID");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Drivers_driverID",
                table: "Booking",
                column: "driverID",
                principalTable: "Drivers",
                principalColumn: "driverID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Drivers_driverID",
                table: "Booking");

            migrationBuilder.DropTable(
                name: "CustomerRatings");

            migrationBuilder.DropTable(
                name: "DriverAllowedCategories");

            migrationBuilder.DropTable(
                name: "DriverAvailabilities");

            migrationBuilder.DropTable(
                name: "DriverLocationPings");

            migrationBuilder.DropTable(
                name: "Trips");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Booking_driverID",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "extraDriverFeePerDay",
                table: "Car");

            migrationBuilder.DropColumn(
                name: "driverDailyFee",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "driverID",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "hasDriver",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "pickupAddress",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "pickupDateTime",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "pickupLatitude",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "pickupLocationLabel",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "pickupLongitude",
                table: "Booking");

            migrationBuilder.AddColumn<int>(
                name: "creditcardID",
                table: "Payment",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CreditCard",
                columns: table => new
                {
                    creditCardID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cardHolderName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    cardNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    cvv = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    expiryDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCard", x => x.creditCardID);
                });

            migrationBuilder.CreateTable(
                name: "CustomerCreditCard",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreditCardId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerCreditCard", x => new { x.UserId, x.CreditCardId });
                    table.ForeignKey(
                        name: "FK_CustomerCreditCard_CreditCard_CreditCardId",
                        column: x => x.CreditCardId,
                        principalTable: "CreditCard",
                        principalColumn: "creditCardID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerCreditCard_Customer_UserId",
                        column: x => x.UserId,
                        principalTable: "Customer",
                        principalColumn: "userID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCreditCard_CreditCardId",
                table: "CustomerCreditCard",
                column: "CreditCardId");
        }
    }
}
