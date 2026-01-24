using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedSenderIdentitiesAndLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedByUserId",
                table: "EmailTemplates",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            /*
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
                        name: "FK_SenderIdentities_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SenderIdentities_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });
            */

            /*
            migrationBuilder.CreateIndex(
                name: "IX_SenderIdentities_CreatedByUserId",
                table: "SenderIdentities",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SenderIdentities_UpdatedByUserId",
                table: "SenderIdentities",
                column: "UpdatedByUserId");
            */

            // --- SEEDING SENDER IDENTITIES (Idempotent) ---
            
            // 1. Rent A Car System
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM SenderIdentities WHERE Id = 1)
                    UPDATE SenderIdentities SET DisplayName = 'Rent A Car System', FromEmail = 'no-reply@rentacarmohammadmahmoud.shop', IsActive = 1, IsDefault = 1 WHERE Id = 1;
                ELSE
                BEGIN
                    SET IDENTITY_INSERT SenderIdentities ON;
                    INSERT INTO SenderIdentities (Id, DisplayName, FromEmail, IsActive, IsDefault, CreatedAt) 
                    VALUES (1, 'Rent A Car System', 'no-reply@rentacarmohammadmahmoud.shop', 1, 1, '2025-01-24T00:00:00Z');
                    SET IDENTITY_INSERT SenderIdentities OFF;
                END
            ");

            // 2. Rent A Car Support
             migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM SenderIdentities WHERE Id = 2)
                    UPDATE SenderIdentities SET DisplayName = 'Rent A Car Support', FromEmail = 'support@rentacarmohammadmahmoud.shop', IsActive = 1, IsDefault = 0 WHERE Id = 2;
                ELSE
                BEGIN
                    SET IDENTITY_INSERT SenderIdentities ON;
                    INSERT INTO SenderIdentities (Id, DisplayName, FromEmail, IsActive, IsDefault, CreatedAt) 
                    VALUES (2, 'Rent A Car Support', 'support@rentacarmohammadmahmoud.shop', 1, 0, '2025-01-24T00:00:00Z');
                    SET IDENTITY_INSERT SenderIdentities OFF;
                END
            ");

            // 3. Rent A Car Billing
             migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM SenderIdentities WHERE Id = 3)
                    UPDATE SenderIdentities SET DisplayName = 'Rent A Car Billing', FromEmail = 'billing@rentacarmohammadmahmoud.shop', IsActive = 1, IsDefault = 0 WHERE Id = 3;
                ELSE
                BEGIN
                    SET IDENTITY_INSERT SenderIdentities ON;
                    INSERT INTO SenderIdentities (Id, DisplayName, FromEmail, IsActive, IsDefault, CreatedAt) 
                    VALUES (3, 'Rent A Car Billing', 'billing@rentacarmohammadmahmoud.shop', 1, 0, '2025-01-24T00:00:00Z');
                    SET IDENTITY_INSERT SenderIdentities OFF;
                END
            ");

            // 4. Rent A Car Alerts
             migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM SenderIdentities WHERE Id = 4)
                    UPDATE SenderIdentities SET DisplayName = 'Rent A Car Alerts', FromEmail = 'notifications@rentacarmohammadmahmoud.shop', IsActive = 1, IsDefault = 0 WHERE Id = 4;
                ELSE
                BEGIN
                    SET IDENTITY_INSERT SenderIdentities ON;
                    INSERT INTO SenderIdentities (Id, DisplayName, FromEmail, IsActive, IsDefault, CreatedAt) 
                    VALUES (4, 'Rent A Car Alerts', 'notifications@rentacarmohammadmahmoud.shop', 1, 0, '2025-01-24T00:00:00Z');
                    SET IDENTITY_INSERT SenderIdentities OFF;
                END
            ");

            // --- LINKING FEATURES TO SENDER IDENTITIES ---
            
            // System (ID 1)
            migrationBuilder.Sql("UPDATE EmailFeatureConfigs SET SenderIdentityId = 1 WHERE FeatureKey IN ('VerifyEmail', 'Otp2FA', 'ForgotPassword', 'ResetPasswordFromSettings', 'AccountStatusChanged')");

            // Support (ID 2)
            migrationBuilder.Sql("UPDATE EmailFeatureConfigs SET SenderIdentityId = 2 WHERE FeatureKey IN ('BookingStatusChanged', 'DocumentStatusUpdate', 'PickupReminder', 'ReturnReminder')");

            // Billing (ID 3)
            migrationBuilder.Sql("UPDATE EmailFeatureConfigs SET SenderIdentityId = 3 WHERE FeatureKey IN ('PaymentInvoice', 'PaymentFailed', 'PaymentReminder')");

            // Alerts (ID 4)
            migrationBuilder.Sql("UPDATE EmailFeatureConfigs SET SenderIdentityId = 4 WHERE FeatureKey IN ('PromoExpiryInternal', 'CarUpdatedInternal', 'CategoryUpdatedInternal', 'PromocodeUpdatedInternal', 'UnverifiedDocsReminderInternal')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SenderIdentities");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedByUserId",
                table: "EmailTemplates",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "Id", "Body", "Category", "IsActive", "Name", "Subject", "TemplateKey", "UpdatedAt", "UpdatedByUserId" },
                values: new object[] { 18, "<!doctype html>\r\n<html>\r\n<head>\r\n  <meta charset=\"utf-8\" />\r\n  <meta name=\"viewport\" content=\"width=device-width,initial-scale=1\" />\r\n  <title>Payment Invoice</title>\r\n</head>\r\n<body style=\"margin:0;padding:0;background:#050505;\">\r\n  <table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:#050505;padding:40px 16px;font-family:Manrope,Segoe UI,Arial,sans-serif;\">\r\n    <tr>\r\n      <td align=\"center\">\r\n        <table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:620px;background:#121212;border:1px solid #2a2a2a;border-radius:16px;overflow:hidden;\">\r\n          <tr>\r\n            <td style=\"background:#0a0a0a;padding:26px 28px;text-align:center;border-bottom:1px solid #2a2a2a;\">\r\n              <div style=\"font-size:22px;font-weight:900;letter-spacing:2px;text-transform:uppercase;color:#edbc1d;\">RentLuxury</div>\r\n              <div style=\"margin-top:6px;font-size:11px;letter-spacing:1.6px;text-transform:uppercase;color:#888888;\">Premium Car Rental</div>\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"padding:32px 28px;color:#ffffff;\">\r\n              <div style=\"display:flex;justify-content:space-between;align-items:start;margin-bottom:24px;\">\r\n                <div>\r\n                  <h2 style=\"margin:0;font-size:24px;font-weight:900;\">Invoice</h2>\r\n                  <p style=\"margin:4px 0 0 0;color:#edbc1d;font-size:12px;font-weight:700;\">#{{InvoiceNumber}}</p>\r\n                </div>\r\n                <div style=\"background:rgba(16,185,129,0.1);color:#10b981;padding:6px 14px;border-radius:20px;font-size:11px;font-weight:900;text-transform:uppercase;letter-spacing:1px;border:1px solid rgba(16,185,129,0.2);\">\r\n                  Paid\r\n                </div>\r\n              </div>\r\n\r\n              <p style=\"margin:0 0 16px 0;color:#a3a3a3;font-size:14px;\">\r\n                Hello <span style=\"color:#ffffff;font-weight:800;\">{{CustomerName}}</span>,\r\n              </p>\r\n              <p style=\"margin:0 0 24px 0;color:#a3a3a3;font-size:14px;line-height:1.6;\">\r\n                Thank you for your payment. Your transaction was successful, and your booking is confirmed. Below are the details of your invoice.\r\n              </p>\r\n\r\n              <div style=\"background:#0a0a0a;border:1px solid #2a2a2a;border-radius:12px;padding:20px;margin-bottom:24px;\">\r\n                <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\">\r\n                  <tr>\r\n                    <td style=\"padding-bottom:12px;color:#888888;font-size:11px;text-transform:uppercase;letter-spacing:1px;\">Description</td>\r\n                    <td align=\"right\" style=\"padding-bottom:12px;color:#888888;font-size:11px;text-transform:uppercase;letter-spacing:1px;\">Amount</td>\r\n                  </tr>\r\n                  <tr>\r\n                    <td style=\"padding:12px 0;color:#ffffff;font-weight:700;border-top:1px solid #2a2a2a;\">\r\n                      Rental Service - {{CarModel}}<br>\r\n                      <span style=\"font-weight:400;color:#888888;font-size:12px;\">{{StartDate}} to {{EndDate}}</span>\r\n                    </td>\r\n                    <td align=\"right\" style=\"padding:12px 0;color:#ffffff;font-weight:900;border-top:1px solid #2a2a2a;\">{{Amount}}</td>\r\n                  </tr>\r\n                  @if(!string.IsNullOrEmpty(\"{{PromoCode}}\")) {\r\n                  <tr>\r\n                    <td style=\"padding:12px 0;color:#10b981;font-size:13px;\">Discount ({{PromoCode}})</td>\r\n                    <td align=\"right\" style=\"padding:12px 0;color:#10b981;font-weight:700;\">-{{DiscountAmount}}</td>\r\n                  </tr>\r\n                  }\r\n                  <tr>\r\n                    <td style=\"padding:18px 0 0 0;color:#ffffff;font-size:16px;font-weight:900;border-top:2px solid #edbc1d;\">Total Paid</td>\r\n                    <td align=\"right\" style=\"padding:18px 0 0 0;color:#edbc1d;font-size:20px;font-weight:900;border-top:2px solid #edbc1d;\">{{TotalPaid}}</td>\r\n                  </tr>\r\n                </table>\r\n              </div>\r\n\r\n              <div style=\"background:rgba(237,188,29,0.05);border:1px solid rgba(237,188,29,0.1);border-radius:12px;padding:16px;\">\r\n                <div style=\"color:#888888;font-size:11px;text-transform:uppercase;letter-spacing:1px;margin-bottom:8px;\">Payment Method</div>\r\n                <div style=\"color:#ffffff;font-size:14px;font-weight:700;\">{{PaymentMethod}}</div>\r\n                <div style=\"margin-top:12px;color:#888888;font-size:11px;text-transform:uppercase;letter-spacing:1px;margin-bottom:8px;\">Transaction Date</div>\r\n                <div style=\"color:#ffffff;font-size:14px;font-weight:700;\">{{PaymentDate}}</div>\r\n              </div>\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"background:#0a0a0a;padding:22px;text-align:center;border-top:1px solid #2a2a2a;\">\r\n              <div style=\"color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;\">\r\n                &copy; {{Year}} RentLuxury Systems • Premium Mobility Solution\r\n              </div>\r\n            </td>\r\n          </tr>\r\n        </table>\r\n      </td>\r\n    </tr>\r\n  </table>\r\n</body>\r\n</html>", "Customer", true, "Payment Invoice", "Your RentLuxury Payment Invoice", "CUST-PAY-INVOICE", new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), null });
        }
    }
}
