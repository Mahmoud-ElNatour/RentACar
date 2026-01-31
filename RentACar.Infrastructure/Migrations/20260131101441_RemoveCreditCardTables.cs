using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCreditCardTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the CustomerCreditCard join table first (has FK to CreditCard)
            migrationBuilder.DropTable(
                name: "CustomerCreditCard");

            // Drop the CreditCard table
            migrationBuilder.DropTable(
                name: "CreditCard");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // We don't want to recreate these tables - credit card functionality is permanently removed
            // If needed, a manual restore from backup would be required
        }
    }
}
