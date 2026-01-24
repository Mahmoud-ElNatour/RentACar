using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Migrations.ApplicationDb
{
    public partial class SeedDefaultDistributionLists : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var now = DateTime.UtcNow;

            // 1. System Administrators
            migrationBuilder.InsertData(
                table: "DistributionLists",
                columns: new[] { "Name", "Description", "IsActive", "CreatedAt" },
                values: new object[] { "System Administrators", "Receives all critical system alerts and configuration changes.", true, now });

            // 2. Fleet Managers
            migrationBuilder.InsertData(
                table: "DistributionLists",
                columns: new[] { "Name", "Description", "IsActive", "CreatedAt" },
                values: new object[] { "Fleet Managers", "Receives vehicle status updates and fleet maintenance alerts.", true, now });

            // 3. Compliance Team
            migrationBuilder.InsertData(
                table: "DistributionLists",
                columns: new[] { "Name", "Description", "IsActive", "CreatedAt" },
                values: new object[] { "Compliance Team", "Receives document verification alerts and compliance reports.", true, now });
                
            // 4. Marketing Team
             migrationBuilder.InsertData(
                table: "DistributionLists",
                columns: new[] { "Name", "Description", "IsActive", "CreatedAt" },
                values: new object[] { "Marketing Team", "Receives promotion expiry alerts and campaign stats.", true, now });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DistributionLists",
                keyColumn: "Name",
                keyValues: new object[] { "System Administrators", "Fleet Managers", "Compliance Team", "Marketing Team" });
        }
    }
}
