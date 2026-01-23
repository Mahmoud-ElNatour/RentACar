using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class EmailConfigCenterFinalize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailProviderSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SenderDomain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultReplyToEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SandboxModeEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RateLimitPerMinute = table.Column<int>(type: "int", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    RetryDelayMinutes = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailProviderSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailProviderSettings_AspNetUser_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SenderIdentities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FromEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReplyToEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SenderIdentities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SenderIdentities_AspNetUser_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SenderIdentities_AspNetUser_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmailFeatureConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeatureKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FeatureName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    SenderIdentityId = table.Column<int>(type: "int", nullable: true),
                    TemplateKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplyToOverride = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailFeatureConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailFeatureConfigs_SenderIdentities_SenderIdentityId",
                        column: x => x.SenderIdentityId,
                        principalTable: "SenderIdentities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ServiceRunRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RunAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TriggeredBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessedCount = table.Column<int>(type: "int", nullable: false),
                    SentCount = table.Column<int>(type: "int", nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRunRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRunItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceRunRecordId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecipientEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRunItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRunItems_ServiceRunRecords_ServiceRunRecordId",
                        column: x => x.ServiceRunRecordId,
                        principalTable: "ServiceRunRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

 // NotificationLogs table already exists

// EmailLogs table already exists

// NotificationSettings table already exists

            migrationBuilder.CreateIndex(
                name: "IX_EmailFeatureConfigs_SenderIdentityId",
                table: "EmailFeatureConfigs",
                column: "SenderIdentityId");

// Index IX_EmailLogs_CreatedByUserId already exists

            migrationBuilder.CreateIndex(
                name: "IX_EmailProviderSettings_UpdatedByUserId",
                table: "EmailProviderSettings",
                column: "UpdatedByUserId");

// Index IX_NotificationSettings_UpdatedByUserId already exists

            migrationBuilder.CreateIndex(
                name: "IX_SenderIdentities_CreatedByUserId",
                table: "SenderIdentities",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SenderIdentities_UpdatedByUserId",
                table: "SenderIdentities",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRunItems_ServiceRunRecordId",
                table: "ServiceRunItems",
                column: "ServiceRunRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailFeatureConfigs");

            migrationBuilder.DropTable(
                name: "ServiceRunItems");

            migrationBuilder.DropTable(
                name: "ServiceRunRecords");

            migrationBuilder.DropTable(
                name: "SenderIdentities");

            migrationBuilder.DropTable(
                name: "EmailProviderSettings");

             migrationBuilder.DropTable(
                name: "NotificationLogs");

             migrationBuilder.DropTable(
                name: "EmailLogs");

            migrationBuilder.DropTable(
                name: "NotificationSettings");
        }
    }
}
