using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Data.Migrations
{
    public partial class AddDriverEmployeeLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "employeeID",
                table: "Drivers",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE d
                SET d.employeeID = e.employeeID
                FROM Drivers d
                INNER JOIN Employees e ON d.aspNetUserId = e.aspNetUserId
            ");

            migrationBuilder.Sql(@"
                DELETE FROM Drivers
                WHERE employeeID IS NULL
            ");

            migrationBuilder.AlterColumn<int>(
                name: "employeeID",
                table: "Drivers",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_employeeID",
                table: "Drivers",
                column: "employeeID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Employees",
                table: "Drivers",
                column: "employeeID",
                principalTable: "Employees",
                principalColumn: "employeeID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Employees",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_employeeID",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "employeeID",
                table: "Drivers");
        }
    }
}
