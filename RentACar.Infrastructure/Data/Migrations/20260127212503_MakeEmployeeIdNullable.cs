using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeEmployeeIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerRatings_Employees_employeeID",
                table: "CustomerRatings");

            migrationBuilder.AlterColumn<int>(
                name: "employeeID",
                table: "CustomerRatings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerRatings_Employees_employeeID",
                table: "CustomerRatings",
                column: "employeeID",
                principalTable: "Employees",
                principalColumn: "employeeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerRatings_Employees_employeeID",
                table: "CustomerRatings");

            migrationBuilder.AlterColumn<int>(
                name: "employeeID",
                table: "CustomerRatings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerRatings_Employees_employeeID",
                table: "CustomerRatings",
                column: "employeeID",
                principalTable: "Employees",
                principalColumn: "employeeID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
