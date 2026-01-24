using System;
using Microsoft.EntityFrameworkCore.Migrations;
using RentACar.Core.Entities;

#nullable disable

namespace RentACar.Infrastructure.Migrations.ApplicationDb
{
    public partial class SeedInvoiceTemplateAndLinkFeatures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var now = DateTime.UtcNow;

            // 1. Insert Payment Invoice Template
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "CUST-PAY-INVOICE",
                    "Payment Invoice",
                    "Customer",
                    "Your RentLuxury Payment Invoice",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <title>Payment Invoice</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#050505;padding:40px 16px;font-family:Manrope,Segoe UI,Arial,sans-serif;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:620px;background:#121212;border:1px solid #2a2a2a;border-radius:16px;overflow:hidden;"">
          <tr>
            <td style=""background:#0a0a0a;padding:26px 28px;text-align:center;border-bottom:1px solid #2a2a2a;"">
              <div style=""font-size:22px;font-weight:900;letter-spacing:2px;text-transform:uppercase;color:#edbc1d;"">RentLuxury</div>
              <div style=""margin-top:6px;font-size:11px;letter-spacing:1.6px;text-transform:uppercase;color:#888888;"">Premium Car Rental</div>
            </td>
          </tr>
          <tr>
            <td style=""padding:32px 28px;color:#ffffff;"">
              <div style=""display:flex;justify-content:space-between;align-items:start;margin-bottom:24px;"">
                <div>
                  <h2 style=""margin:0;font-size:24px;font-weight:900;"">Invoice</h2>
                  <p style=""margin:4px 0 0 0;color:#edbc1d;font-size:12px;font-weight:700;"">#{{InvoiceNumber}}</p>
                </div>
                <div style=""background:rgba(16,185,129,0.1);color:#10b981;padding:6px 14px;border-radius:20px;font-size:11px;font-weight:900;text-transform:uppercase;letter-spacing:1px;border:1px solid rgba(16,185,129,0.2);"">
                  Paid
                </div>
              </div>

              <p style=""margin:0 0 16px 0;color:#a3a3a3;font-size:14px;"">
                Hello <span style=""color:#ffffff;font-weight:800;"">{{CustomerName}}</span>,
              </p>
              <p style=""margin:0 0 24px 0;color:#a3a3a3;font-size:14px;line-height:1.6;"">
                Thank you for your payment. Your transaction was successful, and your booking is confirmed. Below are the details of your invoice.
              </p>

              <div style=""background:#0a0a0a;border:1px solid #2a2a2a;border-radius:12px;padding:20px;margin-bottom:24px;"">
                <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                  <tr>
                    <td style=""padding-bottom:12px;color:#888888;font-size:11px;text-transform:uppercase;letter-spacing:1px;"">Description</td>
                    <td align=""right"" style=""padding-bottom:12px;color:#888888;font-size:11px;text-transform:uppercase;letter-spacing:1px;"">Amount</td>
                  </tr>
                  <tr>
                    <td style=""padding:12px 0;color:#ffffff;font-weight:700;border-top:1px solid #2a2a2a;"">
                      Rental Service - {{CarModel}}<br>
                      <span style=""font-weight:400;color:#888888;font-size:12px;"">{{StartDate}} to {{EndDate}}</span>
                    </td>
                    <td align=""right"" style=""padding:12px 0;color:#ffffff;font-weight:900;border-top:1px solid #2a2a2a;"">{{Amount}}</td>
                  </tr>
                  @if(!string.IsNullOrEmpty(""{{PromoCode}}"")) {
                  <tr>
                    <td style=""padding:12px 0;color:#10b981;font-size:13px;"">Discount ({{PromoCode}})</td>
                    <td align=""right"" style=""padding:12px 0;color:#10b981;font-weight:700;"">-{{DiscountAmount}}</td>
                  </tr>
                  }
                  <tr>
                    <td style=""padding:18px 0 0 0;color:#ffffff;font-size:16px;font-weight:900;border-top:2px solid #edbc1d;"">Total Paid</td>
                    <td align=""right"" style=""padding:18px 0 0 0;color:#edbc1d;font-size:20px;font-weight:900;border-top:2px solid #edbc1d;"">{{TotalPaid}}</td>
                  </tr>
                </table>
              </div>

              <div style=""background:rgba(237,188,29,0.05);border:1px solid rgba(237,188,29,0.1);border-radius:12px;padding:16px;"">
                <div style=""color:#888888;font-size:11px;text-transform:uppercase;letter-spacing:1px;margin-bottom:8px;"">Payment Method</div>
                <div style=""color:#ffffff;font-size:14px;font-weight:700;"">{{PaymentMethod}}</div>
                <div style=""margin-top:12px;color:#888888;font-size:11px;text-transform:uppercase;letter-spacing:1px;margin-bottom:8px;"">Transaction Date</div>
                <div style=""color:#ffffff;font-size:14px;font-weight:700;"">{{PaymentDate}}</div>
              </div>
            </td>
          </tr>
          <tr>
            <td style=""background:#0a0a0a;padding:22px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems • Premium Mobility Solution
              </div>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>",
                    true, now
                });

            // 2. Link Templates to Features
            var featureMappings = new System.Collections.Generic.Dictionary<string, string>
            {
                { "VerifyEmail", "AUTH-V2-VERIFY" },
                { "Otp2FA", "AUTH-OTP-SECURE" },
                { "ForgotPassword", "AUTH-RESET-V1" },
                { "ResetPasswordFromSettings", "AUTH-PWD-CHANGE" },
                { "AccountStatusChanged", "AUTH-ACC-STATUS" },
                { "BookingStatusChanged", "CUST-BOOK-UPDATE" },
                { "PaymentFailed", "CUST-PAY-FAILED" },
                { "PaymentInvoice", "CUST-PAY-INVOICE" },
                { "DocumentStatusUpdate", "CUST-DOC-VERIFY" },
                { "PaymentReminder", "REM-PAY-GENERIC" },
                { "PickupReminder", "REM-PICK-INSTR" },
                { "ReturnReminder", "REM-RET-INSTR" },
                { "PromoExpiryInternal", "INT-PROMO-EXP" },
                { "CarUpdatedInternal", "INT-CAR-UPD" },
                { "CategoryUpdatedInternal", "INT-CAT-UPD" },
                { "PromocodeUpdatedInternal", "INT-PROMOC-UPD" },
                { "UnverifiedDocsReminderInternal", "INT-DOCS-UNV" }
            };

            foreach (var mapping in featureMappings)
            {
                migrationBuilder.Sql($"UPDATE EmailFeatureConfigs SET TemplateKey = '{mapping.Value}' WHERE FeatureKey = '{mapping.Key}' AND (TemplateKey IS NULL OR TemplateKey = '')");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Remove Invoice Template
            migrationBuilder.DeleteData(
                table: "EmailTemplates",
                keyColumn: "TemplateKey",
                keyValue: "CUST-PAY-INVOICE");

            // 2. Unlink Features (Reset to NULL)
             var featureKeys = new[] { "VerifyEmail", "Otp2FA", "ForgotPassword", "ResetPasswordFromSettings", "AccountStatusChanged", 
                                       "BookingStatusChanged", "PaymentFailed", "PaymentInvoice", "DocumentStatusUpdate", 
                                       "PaymentReminder", "PickupReminder", "ReturnReminder", 
                                       "PromoExpiryInternal", "CarUpdatedInternal", "CategoryUpdatedInternal", "PromocodeUpdatedInternal", "UnverifiedDocsReminderInternal" };

             foreach(var key in featureKeys)
             {
                 migrationBuilder.Sql($"UPDATE EmailFeatureConfigs SET TemplateKey = NULL WHERE FeatureKey = '{key}'");
             }
        }
    }
}
