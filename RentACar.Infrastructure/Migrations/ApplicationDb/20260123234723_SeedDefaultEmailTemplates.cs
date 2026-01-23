using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class SeedDefaultEmailTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var now = DateTime.UtcNow;
            
            // Helper to keep SQL cleaner
            var style = @"font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; line-height: 1.6; color: #333;";
            var btnStyle = @"display: inline-block; padding: 12px 24px; background-color: #d4af37; color: #000; text-decoration: none; border-radius: 4px; font-weight: bold;";
            var footerStyle = @"margin-top: 30px; font-size: 12px; color: #999; border-top: 1px solid #eee; padding-top: 20px;";

            // 1. AUTH-V2-VERIFY
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "AUTH-V2-VERIFY",
                    "Email Verification (V2)",
                    "Authentication",
                    "Verify your email • RentLuxury",
                    $@"<div style=""{style}""><h2>Welcome to RentLuxury</h2><p>Hello {{CustomerName}},</p><p>Please use the verification code below to activate your account:</p><div style=""font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #d4af37; margin: 20px 0;"">{{OtpCode}}</div><p>This code will expire in 15 minutes.</p><p style=""{footerStyle}"">RentLuxury Security Team</p></div>",
                    true, now
                });

            // 2. AUTH-OTP-SECURE
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "AUTH-OTP-SECURE",
                    "One-Time Password",
                    "Authentication",
                    "Your Secure Login Code",
                    $@"<div style=""{style}""><h2>Login Attempt</h2><p>Hello {{CustomerName}},</p><p>A login attempt was detected for your account.</p><p>Your One-Time Password is:</p><div style=""font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #d4af37; margin: 20px 0;"">{{OtpCode}}</div><p>If this wasn't you, please contact support immediately.</p><p style=""{footerStyle}"">RentLuxury Security Team</p></div>",
                    true, now
                });
                
            // 3. AUTH-RESET-V1
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "AUTH-RESET-V1",
                    "Forgot Password",
                    "Authentication",
                    "Reset your password",
                    $@"<div style=""{style}""><h2>Password Reset Request</h2><p>Hello {{CustomerName}},</p><p>We received a request to reset your password. Click the button below to proceed:</p><p><a href=""{{ActionUrl}}"" style=""{btnStyle}"">Reset Password</a></p><p>Or use this link: {{ActionUrl}}</p><p>If you didn't ask for this, you can ignore this email.</p><p style=""{footerStyle}"">RentLuxury Support</p></div>",
                    true, now
                });

            // 4. AUTH-PWD-CHANGE
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "AUTH-PWD-CHANGE",
                    "Password Changed Notification",
                    "Authentication",
                    "Security Alert: Password Changed",
                    $@"<div style=""{style}""><h2>Password Updated</h2><p>Hello {{CustomerName}},</p><p>Your account password was recently changed.</p><p>If you did not perform this action, please secure your account immediately.</p><p style=""{footerStyle}"">RentLuxury Security Team</p></div>",
                    true, now
                });

            // 5. AUTH-ACC-STATUS
             migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "AUTH-ACC-STATUS",
                    "Account Status Update",
                    "Authentication",
                    "Important: Account Status Update",
                    $@"<div style=""{style}""><h2>Account Update</h2><p>Hello {{CustomerName}},</p><p>Your account status has been updated to: <strong>{{AccountStatus}}</strong>.</p><p>Reason: {{StatusReason}}</p><p>Please contact us if you have questions.</p><p style=""{footerStyle}"">RentLuxury Operations</p></div>",
                    true, now
                });

            // 6. CUST-BOOK-UPDATE
             migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "CUST-BOOK-UPDATE",
                    "Booking Status Changed",
                    "Customer",
                    "Update on Booking #{{BookingId}}",
                    $@"<div style=""{style}""><h2>Booking Update</h2><p>Hello {{CustomerName}},</p><p>The status of your booking <strong>#{{BookingId}}</strong> has changed to: <span style=""color:#d4af37; font-weight:bold"">{{BookingStatus}}</span>.</p><p>Vehicle: {{CarModel}}</p><p>Dates: {{StartDate}} to {{EndDate}}</p><p style=""{footerStyle}"">RentLuxury Concierge</p></div>",
                    true, now
                });

            // 7. CUST-PAY-FAILED
             migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "CUST-PAY-FAILED",
                    "Payment Failed Alert",
                    "Customer",
                    "Action Required: Payment Failed",
                    $@"<div style=""{style}""><h2>Payment Declined</h2><p>Hello {{CustomerName}},</p><p>We were unable to process the payment of <strong>{{Amount}}</strong> for Booking #{{BookingId}}.</p><p>Please update your payment method to avoid cancellation.</p><p><a href=""{{PaymentUrl}}"" style=""{btnStyle}"">Update Payment</a></p><p style=""{footerStyle}"">RentLuxury Billing</p></div>",
                    true, now
                });

            // 8. CUST-DOC-VERIFY
             migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "CUST-DOC-VERIFY",
                    "Document Verification Result",
                    "Customer",
                    "Document Verification Update",
                    $@"<div style=""{style}""><h2>Document Status</h2><p>Hello {{CustomerName}},</p><p>Your document ({{DocumentType}}) has been reviewed.</p><p>Status: <strong>{{DocumentStatus}}</strong></p><p>{{RejectionReason}}</p><p style=""{footerStyle}"">RentLuxury Compliance</p></div>",
                    true, now
                });

            // 9. REM-PAY-GENERIC
             migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "REM-PAY-GENERIC",
                    "Payment Reminder",
                    "Reminder",
                    "Reminder: Payment Due for #{{BookingId}}",
                    $@"<div style=""{style}""><h2>Payment Reminder</h2><p>Hello {{CustomerName}},</p><p>This is a reminder that a payment of <strong>{{Amount}}</strong> for your booking is pending.</p><p>Please settle this invoice to secure your reservation.</p><p><a href=""{{PaymentUrl}}"" style=""{btnStyle}"">Pay Now</a></p><p style=""{footerStyle}"">RentLuxury Billing</p></div>",
                    true, now
                });

            // 10. REM-PICK-INSTR
             migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "REM-PICK-INSTR",
                    "Pickup Instructions",
                    "Reminder",
                    "Your Upcoming Pickup: {{CarModel}}",
                    $@"<div style=""{style}""><h2>Get Ready for Your Trip</h2><p>Hello {{CustomerName}},</p><p>Your booking <strong>#{{BookingId}}</strong> starts soon.</p><ul><li><strong>Vehicle:</strong> {{CarModel}}</li><li><strong>Time:</strong> {{PickupTime}}</li><li><strong>Location:</strong> {{PickupLocation}}</li></ul><p>Please bring your ID and driving license.</p><p style=""{footerStyle}"">RentLuxury Support</p></div>",
                    true, now
                });

            // 11. REM-RET-INSTR
             migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "REM-RET-INSTR",
                    "Return Instructions",
                    "Reminder",
                    "Returning your vehicle",
                    $@"<div style=""{style}""><h2>Vehicle Return</h2><p>Hello {{CustomerName}},</p><p>Your rental for the <strong>{{CarModel}}</strong> ends on {{ReturnDate}}.</p><p>Please return the vehicle to: {{ReturnLocation}}.</p><p>Ensure the tank is full to avoid extra charges.</p><p style=""{footerStyle}"">RentLuxury Support</p></div>",
                    true, now
                });

            // 12. INT-PROMO-EXP
             migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "INT-PROMO-EXP",
                    "Promo Expiry Alert (Internal)",
                    "Internal",
                    "[INTERNAL] Promo Code Expiring: {{PromoCode}}",
                    $@"<div style=""{style}""><h2>Promo Expiry Alert</h2><p>The promo code <strong>{{PromoCode}}</strong> is set to expire on {{ExpiryDate}}.</p><p>Current Usage: {{UsageCount}}</p><p>Please review extension policies if needed.</p><p style=""{footerStyle}"">RentLuxury System</p></div>",
                    true, now
                });

            // 13. INT-CAR-UPD
             migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "INT-CAR-UPD",
                    "Car Updated (Internal)",
                    "Internal",
                    "[INTERNAL] Fleet Update: {{CarModel}}",
                    $@"<div style=""{style}""><h2>Fleet Update</h2><p>Vehicle <strong>{{CarModel}}</strong> ({{PlateNumber}}) details have been updated by {{UpdatedBy}}.</p><p>Changes: {{ChangeSummary}}</p><p style=""{footerStyle}"">RentLuxury System</p></div>",
                    true, now
                });

            // 14. INT-CAT-UPD
             migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "INT-CAT-UPD",
                    "Category Pricing Updated (Internal)",
                    "Internal",
                    "[INTERNAL] Pricing Update: {{CategoryName}}",
                    $@"<div style=""{style}""><h2>Pricing Alert</h2><p>Rental Category <strong>{{CategoryName}}</strong> pricing has changed.</p><p>New Base Price: {{NewPrice}}</p><p>Effective: Immediately</p><p style=""{footerStyle}"">RentLuxury System</p></div>",
                    true, now
                });
                
            // 15. INT-PROMOC-UPD
             migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "INT-PROMOC-UPD",
                    "Promocode Modified (Internal)",
                    "Internal",
                    "[INTERNAL] Promo Modified: {{PromoCode}}",
                    $@"<div style=""{style}""><h2>Promo Modified</h2><p>Promo code <strong>{{PromoCode}}</strong> has been modified.</p><p>Changes: {{ChangeDetail}}</p><p style=""{footerStyle}"">RentLuxury System</p></div>",
                    true, now
                });

            // 16. INT-DOCS-UNV
             migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "INT-DOCS-UNV",
                    "Unverified Docs Report (Internal)",
                    "Internal",
                    "[INTERNAL] Daily Unverified Docs Report",
                    $@"<div style=""{style}""><h2>Compliance Report</h2><p>There are <strong>{{Count}}</strong> users with unverified documents pending for more than 24 hours.</p><p>Please review the queue.</p><p style=""{footerStyle}"">RentLuxury System</p></div>",
                    true, now
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EmailTemplates",
                keyColumn: "TemplateKey",
                keyValues: new object[] {
                    "AUTH-V2-VERIFY", "AUTH-OTP-SECURE", "AUTH-RESET-V1", "AUTH-PWD-CHANGE", "AUTH-ACC-STATUS",
                    "CUST-BOOK-UPDATE", "CUST-PAY-FAILED", "CUST-DOC-VERIFY",
                    "REM-PAY-GENERIC", "REM-PICK-INSTR", "REM-RET-INSTR",
                    "INT-PROMO-EXP", "INT-CAR-UPD", "INT-CAT-UPD", "INT-PROMOC-UPD", "INT-DOCS-UNV"
                });
        }
    }
}
