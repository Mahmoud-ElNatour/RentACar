using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerCreditCardCompositeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerCreditCard",
                table: "CustomerCreditCard");

            migrationBuilder.DropIndex(
                name: "IX_CustomerCreditCard_userId",
                table: "CustomerCreditCard");

            migrationBuilder.DropColumn(
                name: "CustomerCreditCardId",
                table: "CustomerCreditCard");

            migrationBuilder.AlterColumn<int>(
                name: "userId",
                table: "CustomerCreditCard",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AlterColumn<int>(
                name: "creditCardId",
                table: "CustomerCreditCard",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerCreditCard",
                table: "CustomerCreditCard",
                columns: new[] { "userId", "creditCardId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerCreditCard",
                table: "CustomerCreditCard");

            migrationBuilder.AlterColumn<int>(
                name: "creditCardId",
                table: "CustomerCreditCard",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "userId",
                table: "CustomerCreditCard",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddColumn<int>(
                name: "CustomerCreditCardId",
                table: "CustomerCreditCard",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerCreditCard",
                table: "CustomerCreditCard",
                column: "CustomerCreditCardId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCreditCard_userId",
                table: "CustomerCreditCard",
                column: "userId");
        }
    }
}
