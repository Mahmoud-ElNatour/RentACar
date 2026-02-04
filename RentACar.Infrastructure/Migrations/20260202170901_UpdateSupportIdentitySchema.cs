using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSupportIdentitySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportConversations_AspNetUsers_AssignedEmployeeId",
                table: "SupportConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportConversations_AspNetUsers_CustomerId",
                table: "SupportConversations");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "SupportConversations",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<int>(
                name: "AssignedEmployeeId",
                table: "SupportConversations",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportConversations_Customers_CustomerId",
                table: "SupportConversations",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "userID");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportConversations_Employees_AssignedEmployeeId",
                table: "SupportConversations",
                column: "AssignedEmployeeId",
                principalTable: "Employees",
                principalColumn: "employeeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportConversations_Customers_CustomerId",
                table: "SupportConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportConversations_Employees_AssignedEmployeeId",
                table: "SupportConversations");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "SupportConversations",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedEmployeeId",
                table: "SupportConversations",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportConversations_AspNetUsers_AssignedEmployeeId",
                table: "SupportConversations",
                column: "AssignedEmployeeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportConversations_AspNetUsers_CustomerId",
                table: "SupportConversations",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
