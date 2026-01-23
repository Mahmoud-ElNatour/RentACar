using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ImplementTemplatesFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "creditCardId",
                table: "CustomerCreditCard",
                newName: "CreditCardId");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "CustomerCreditCard",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerCreditCard_creditCardId",
                table: "CustomerCreditCard",
                newName: "IX_CustomerCreditCard_CreditCardId");

            migrationBuilder.AlterColumn<int>(
                name: "CreditCardId",
                table: "CustomerCreditCard",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "CustomerCreditCard",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreditCardId",
                table: "CustomerCreditCard",
                newName: "creditCardId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "CustomerCreditCard",
                newName: "userId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerCreditCard_CreditCardId",
                table: "CustomerCreditCard",
                newName: "IX_CustomerCreditCard_creditCardId");

            migrationBuilder.AlterColumn<int>(
                name: "creditCardId",
                table: "CustomerCreditCard",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "userId",
                table: "CustomerCreditCard",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 0);
        }
    }
}
