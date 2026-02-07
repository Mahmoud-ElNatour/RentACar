using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverAllowedCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                        name: "FK_DriverAllowedCategories_Categories_categoryID",
                        column: x => x.categoryID,
                        principalTable: "Categories",
                        principalColumn: "categoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DriverAllowedCategories_Drivers_driverID",
                        column: x => x.driverID,
                        principalTable: "Drivers",
                        principalColumn: "driverID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverAllowedCategories_categoryID",
                table: "DriverAllowedCategories",
                column: "categoryID");

            migrationBuilder.CreateIndex(
                name: "IX_DriverAllowedCategories_driverID_categoryID",
                table: "DriverAllowedCategories",
                columns: new[] { "driverID", "categoryID" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverAllowedCategories");
        }
    }
}
