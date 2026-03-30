using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCarSpecifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "doors",
                table: "Cars",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "fuelType",
                table: "Cars",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "hasGPS",
                table: "Cars",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "hasInfotainmentScreen",
                table: "Cars",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "hasParkingSensors",
                table: "Cars",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "hasRearCamera",
                table: "Cars",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "hasSunroof",
                table: "Cars",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "luggageCapacity",
                table: "Cars",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "seatsCapacity",
                table: "Cars",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "supportsBabySeat",
                table: "Cars",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "transmissionType",
                table: "Cars",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "doors",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "fuelType",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "hasGPS",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "hasInfotainmentScreen",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "hasParkingSensors",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "hasRearCamera",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "hasSunroof",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "luggageCapacity",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "seatsCapacity",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "supportsBabySeat",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "transmissionType",
                table: "Cars");
        }
    }
}
