using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailServicesEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DistributionLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributionLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailDrafts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecipientsRaw = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelectedDistributionListIdsRaw = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailDrafts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecipientsRaw = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecipientEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Actor = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReminderProcessingEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AllNotificationEmailsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CheckIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    PaymentReminderEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PaymentReminderDelayHours = table.Column<int>(type: "int", nullable: false),
                    PaymentReminderRepeatEveryHours = table.Column<int>(type: "int", nullable: true),
                    PaymentReminderMaxSends = table.Column<int>(type: "int", nullable: true),
                    PickupReminderEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PickupReminderHoursBefore = table.Column<int>(type: "int", nullable: false),
                    ReturnReminderEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ReturnReminderHoursBefore = table.Column<int>(type: "int", nullable: false),
                    PromoExpiryEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PromoExpiryAutoDeactivate = table.Column<bool>(type: "bit", nullable: false),
                    PromoExpiryCheckFrequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeesDefaultListId = table.Column<int>(type: "int", nullable: true),
                    PromoExpiryEmployeesListId = table.Column<int>(type: "int", nullable: true),
                    CarUpdateEmployeesListId = table.Column<int>(type: "int", nullable: true),
                    CategoryUpdateEmployeesListId = table.Column<int>(type: "int", nullable: true),
                    PromocodeUpdateEmployeesListId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DistributionListMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DistributionListId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MemberType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributionListMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DistributionListMembers_DistributionLists_DistributionListId",
                        column: x => x.DistributionListId,
                        principalTable: "DistributionLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DistributionListRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DistributionListId = table.Column<int>(type: "int", nullable: false),
                    IncludeEmployees = table.Column<bool>(type: "bit", nullable: false),
                    IncludeAdmins = table.Column<bool>(type: "bit", nullable: false),
                    IncludeCustomers = table.Column<bool>(type: "bit", nullable: false),
                    OnlyActiveUsers = table.Column<bool>(type: "bit", nullable: false),
                    ExcludeBlacklistedCustomers = table.Column<bool>(type: "bit", nullable: false),
                    OnlyVerifiedEmails = table.Column<bool>(type: "bit", nullable: false),
                    ManualEmailsRaw = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributionListRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DistributionListRules_DistributionLists_DistributionListId",
                        column: x => x.DistributionListId,
                        principalTable: "DistributionLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DistributionListMembers_DistributionListId_Email",
                table: "DistributionListMembers",
                columns: new[] { "DistributionListId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DistributionListRules_DistributionListId",
                table: "DistributionListRules",
                column: "DistributionListId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionLists_Name",
                table: "DistributionLists",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_TemplateKey",
                table: "EmailTemplates",
                column: "TemplateKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DistributionListMembers");

            migrationBuilder.DropTable(
                name: "DistributionListRules");

            migrationBuilder.DropTable(
                name: "EmailDrafts");

            migrationBuilder.DropTable(
                name: "EmailLogs");

            migrationBuilder.DropTable(
                name: "EmailTemplates");

            migrationBuilder.DropTable(
                name: "NotificationLogs");

            migrationBuilder.DropTable(
                name: "NotificationSettings");

            migrationBuilder.DropTable(
                name: "DistributionLists");
        }
    }
}
