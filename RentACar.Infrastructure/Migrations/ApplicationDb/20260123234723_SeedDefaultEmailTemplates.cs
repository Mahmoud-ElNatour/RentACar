using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Migrations.ApplicationDb
{
    public partial class SeedDefaultEmailTemplates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var now = DateTime.UtcNow;

            // 1) AUTH-ACC-STATUS
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "AUTH-ACC-STATUS",
                    "Account Status Update",
                    "Authentication",
                    "Account Status Update",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Account Status Update</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:#050505;line-height:1px;font-size:1px;"">
    Your RentLuxury account status has been updated.
  </div>

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
              <h2 style=""margin:0 0 14px 0;font-size:20px;font-weight:900;letter-spacing:.2px;"">Account Status</h2>

              <p style=""margin:0 0 16px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Hello <span style=""color:#ffffff;font-weight:800;"">{{CustomerName}}</span>,
              </p>

              <p style=""margin:0 0 18px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Your account status has been updated. See details below:
              </p>

              <div style=""background:#0a0a0a;border:1px solid #2a2a2a;border-radius:12px;padding:18px 18px;margin:18px 0;"">
                <div style=""margin:0 0 6px 0;color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">New Status</div>
                <div style=""margin:0 0 14px 0;color:#ffffff;font-size:16px;font-weight:900;"">{{AccountStatus}}</div>

                <div style=""margin:0 0 6px 0;color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">Reason</div>
                <div style=""margin:0;color:#a3a3a3;font-size:14px;line-height:1.7;"">{{StatusReason}}</div>
              </div>

              <p style=""margin:0;color:#888888;font-size:12px;line-height:1.6;"">
                If you believe this is a mistake, contact support immediately.
              </p>
            </td>
          </tr>

          <tr>
            <td style=""background:#0a0a0a;padding:18px 22px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems • All rights reserved
              </div>
            </td>
          </tr>
        </table>

        <div style=""height:18px;""></div>
        <div style=""color:#888888;font-size:12px;line-height:1.6;text-align:center;"">
          If you didn’t request this, you can safely ignore this email.
        </div>
      </td>
    </tr>
  </table>
</body>
</html>",
                    true, now
                });

            // 2) AUTH-V2-VERIFY
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "AUTH-V2-VERIFY",
                    "Email Verification (V2)",
                    "Authentication",
                    "Email Verification",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Email Verification</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:#050505;line-height:1px;font-size:1px;"">
    Your verification code for RentLuxury.
  </div>

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
              <h2 style=""margin:0 0 14px 0;font-size:20px;font-weight:900;"">Confirm Your Email</h2>

              <p style=""margin:0 0 16px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Hello <span style=""color:#ffffff;font-weight:800;"">{{CustomerName}}</span>,
              </p>

              <p style=""margin:0 0 18px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Use the verification code below to confirm your email address:
              </p>

              <div style=""background:#0a0a0a;border:1px solid rgba(237,188,29,0.35);border-radius:12px;padding:18px;margin:18px 0;text-align:center;"">
                <div style=""font-family:ui-monospace,SFMono-Regular,Menlo,Monaco,Consolas,'Liberation Mono','Courier New',monospace;
                            color:#edbc1d;font-size:30px;font-weight:900;letter-spacing:7px;"">
                  {{OtpCode}}
                </div>
              </div>

              <div style=""height:1px;background:#2a2a2a;margin:18px 0;""></div>

              <p style=""margin:0;color:#888888;font-size:12px;line-height:1.6;"">
                This code expires in <span style=""color:#edbc1d;font-weight:900;"">15 minutes</span>.
              </p>
            </td>
          </tr>

          <tr>
            <td style=""background:#0a0a0a;padding:18px 22px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems • All rights reserved
              </div>
            </td>
          </tr>
        </table>

        <div style=""height:18px;""></div>
        <div style=""color:#888888;font-size:12px;line-height:1.6;text-align:center;"">
          If you didn’t request this, you can safely ignore this email.
        </div>
      </td>
    </tr>
  </table>
</body>
</html>",
                    true, now
                });

            // 3) AUTH-RESET-V1
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "AUTH-RESET-V1",
                    "Forgot Password",
                    "Authentication",
                    "Reset Password",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Reset Password</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:#050505;line-height:1px;font-size:1px;"">
    Reset your RentLuxury password.
  </div>

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
              <h2 style=""margin:0 0 14px 0;font-size:20px;font-weight:900;"">Password Reset</h2>

              <p style=""margin:0 0 16px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Hello <span style=""color:#ffffff;font-weight:800;"">{{CustomerName}}</span>,
              </p>

              <p style=""margin:0 0 18px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                We received a request to reset your password. Click the button below to continue:
              </p>

              <div style=""text-align:center;padding:10px 0 4px 0;"">
                <a href=""{{ActionUrl}}"" style=""display:inline-block;padding:14px 30px;background:#edbc1d;color:#000000;text-decoration:none;border-radius:10px;font-weight:900;font-size:12px;text-transform:uppercase;letter-spacing:1px;"">
                  Reset Password
                </a>
              </div>

              <p style=""margin:14px 0 6px 0;color:#888888;font-size:12px;line-height:1.6;"">
                If the button doesn’t work, copy and paste this link:
              </p>
              <p style=""margin:0;color:#a3a3a3;font-size:12px;line-height:1.6;"">
                <a href=""{{ActionUrl}}"" style=""color:#edbc1d;text-decoration:underline;word-break:break-word;"">{{ActionUrl}}</a>
              </p>
            </td>
          </tr>

          <tr>
            <td style=""background:#0a0a0a;padding:18px 22px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems • All rights reserved
              </div>
            </td>
          </tr>
        </table>

        <div style=""height:18px;""></div>
        <div style=""color:#888888;font-size:12px;line-height:1.6;text-align:center;"">
          If you didn’t request this, you can safely ignore this email.
        </div>
      </td>
    </tr>
  </table>
</body>
</html>",
                    true, now
                });

            // 4) AUTH-OTP-SECURE
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "AUTH-OTP-SECURE",
                    "One-Time Password",
                    "Authentication",
                    "Secure Login Code",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Secure Login Code</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:#050505;line-height:1px;font-size:1px;"">
    Your RentLuxury secure login code.
  </div>

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
              <h2 style=""margin:0 0 14px 0;font-size:20px;font-weight:900;"">Secure Login Attempt</h2>

              <p style=""margin:0 0 16px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Hello <span style=""color:#ffffff;font-weight:800;"">{{CustomerName}}</span>,
              </p>

              <p style=""margin:0 0 18px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Use this one-time code to continue signing in:
              </p>

              <div style=""background:#0a0a0a;border:1px solid rgba(237,188,29,0.35);border-radius:12px;padding:18px;margin:18px 0;text-align:center;"">
                <div style=""font-family:ui-monospace,SFMono-Regular,Menlo,Monaco,Consolas,'Liberation Mono','Courier New',monospace;
                            color:#edbc1d;font-size:30px;font-weight:900;letter-spacing:7px;"">
                  {{OtpCode}}
                </div>
              </div>

              <p style=""margin:0;color:#888888;font-size:12px;line-height:1.6;"">
                Never share this code with anyone. Support will never ask for it.
              </p>
            </td>
          </tr>

          <tr>
            <td style=""background:#0a0a0a;padding:18px 22px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems • All rights reserved
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

            // 5) AUTH-PWD-CHANGE
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "AUTH-PWD-CHANGE",
                    "Password Changed",
                    "Authentication",
                    "Password Changed",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Password Changed</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:#050505;line-height:1px;font-size:1px;"">
    Your RentLuxury password was updated.
  </div>

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
              <h2 style=""margin:0 0 14px 0;font-size:20px;font-weight:900;"">Security Alert</h2>

              <p style=""margin:0 0 16px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Hello <span style=""color:#ffffff;font-weight:800;"">{{CustomerName}}</span>,
              </p>

              <p style=""margin:0 0 18px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Your account password has been successfully updated.
              </p>

              <div style=""background:#0a0a0a;border:1px solid #2a2a2a;border-left:3px solid #edbc1d;border-radius:12px;padding:16px 16px;margin:18px 0;"">
                <div style=""color:#ffffff;font-weight:800;margin:0 0 6px 0;"">Didn’t do this?</div>
                <div style=""color:#a3a3a3;font-size:14px;line-height:1.7;"">
                  Contact support immediately to secure your account.
                </div>
              </div>

              <p style=""margin:0;color:#888888;font-size:12px;line-height:1.6;"">
                For your safety, avoid sharing credentials and enable 2FA.
              </p>
            </td>
          </tr>

          <tr>
            <td style=""background:#0a0a0a;padding:18px 22px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems • All rights reserved
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
            
            // 6) CUST-BOOK-UPDATE
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "CUST-BOOK-UPDATE",
                    "Booking Status Changed",
                    "Customer",
                    "Booking Update",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Booking Update</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:#050505;line-height:1px;font-size:1px;"">
    Your booking status has changed.
  </div>

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
              <h2 style=""margin:0 0 14px 0;font-size:20px;font-weight:900;"">Booking Update</h2>

              <p style=""margin:0 0 16px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Hello <span style=""color:#ffffff;font-weight:800;"">{{CustomerName}}</span>,
              </p>

              <p style=""margin:0 0 18px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                The status of your reservation has changed. Here are the details:
              </p>

              <div style=""background:#0a0a0a;border:1px solid #2a2a2a;border-radius:12px;padding:18px;margin:18px 0;"">
                <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">Booking ID</div>
                <div style=""color:#ffffff;font-weight:900;font-size:16px;margin:4px 0 12px 0;"">#{{BookingId}}</div>

                <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">New Status</div>
                <div style=""color:#edbc1d;font-weight:900;font-size:16px;margin:4px 0 12px 0;"">{{BookingStatus}}</div>

                <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">Vehicle</div>
                <div style=""color:#ffffff;font-weight:900;font-size:16px;margin:4px 0 0 0;"">{{CarModel}}</div>
              </div>

              <p style=""margin:0;color:#888888;font-size:12px;line-height:1.6;"">
                Dates: <span style=""color:#edbc1d;font-weight:900;"">{{StartDate}}</span> — <span style=""color:#edbc1d;font-weight:900;"">{{EndDate}}</span>
              </p>
            </td>
          </tr>

          <tr>
            <td style=""background:#0a0a0a;padding:18px 22px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems • All rights reserved
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

            // 7) CUST-DOC-VERIFY
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "CUST-DOC-VERIFY",
                    "Document Verification Result",
                    "Customer",
                    "Document Verification Update",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Document Verification</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:#050505;line-height:1px;font-size:1px;"">
    Your document verification status is available.
  </div>

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
              <h2 style=""margin:0 0 14px 0;font-size:20px;font-weight:900;"">Document Review</h2>

              <p style=""margin:0 0 16px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Hello <span style=""color:#ffffff;font-weight:800;"">{{CustomerName}}</span>,
              </p>

              <p style=""margin:0 0 18px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                We reviewed your uploaded document: <span style=""color:#edbc1d;font-weight:900;"">{{DocumentType}}</span>.
              </p>

              <div style=""background:#0a0a0a;border:1px solid #2a2a2a;border-left:3px solid #edbc1d;border-radius:12px;padding:16px;margin:18px 0;"">
                <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">Status</div>
                <div style=""color:#ffffff;font-weight:900;font-size:16px;margin:4px 0 12px 0;"">{{DocumentStatus}}</div>

                <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">Notes</div>
                <div style=""color:#a3a3a3;font-size:14px;line-height:1.7;margin-top:4px;"">{{RejectionReason}}</div>
              </div>

              <p style=""margin:0;color:#888888;font-size:12px;line-height:1.6;"">
                If you need help, reply to this email or contact support.
              </p>
            </td>
          </tr>

          <tr>
            <td style=""background:#0a0a0a;padding:18px 22px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems • All rights reserved
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
            
            // 8) CUST-PAY-FAILED
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "CUST-PAY-FAILED",
                    "Payment Failed Alert",
                    "Customer",
                    "Payment Failed",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Payment Failed</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:#050505;line-height:1px;font-size:1px;"">
    Payment failed — retry to secure your booking.
  </div>

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
              <h2 style=""margin:0 0 14px 0;font-size:20px;font-weight:900;"">Payment Declined</h2>

              <p style=""margin:0 0 16px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Hello <span style=""color:#ffffff;font-weight:800;"">{{CustomerName}}</span>,
              </p>

              <p style=""margin:0 0 18px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                We were unable to process your payment. Please retry to secure your booking.
              </p>

              <div style=""background:#0a0a0a;border:1px solid rgba(237,188,29,0.25);border-radius:12px;padding:18px;margin:18px 0;text-align:center;"">
                <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">Amount</div>
                <div style=""margin-top:8px;color:#edbc1d;font-size:26px;font-weight:900;"">{{Amount}}</div>
              </div>

              <div style=""text-align:center;padding:10px 0 4px 0;"">
                <a href=""{{PaymentUrl}}"" style=""display:inline-block;padding:14px 30px;background:#edbc1d;color:#000000;text-decoration:none;border-radius:10px;font-weight:900;font-size:12px;text-transform:uppercase;letter-spacing:1px;"">
                  Retry Payment
                </a>
              </div>

              <p style=""margin:14px 0 0 0;color:#888888;font-size:12px;line-height:1.6;"">
                Or open: <a href=""{{PaymentUrl}}"" style=""color:#edbc1d;text-decoration:underline;word-break:break-word;"">{{PaymentUrl}}</a>
              </p>
            </td>
          </tr>

          <tr>
            <td style=""background:#0a0a0a;padding:18px 22px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems • All rights reserved
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
            
            // 9) AUTH-VERIFY-LINK
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "AUTH-VERIFY-LINK",
                    "Verify Email (Link)",
                    "Authentication",
                    "Verify Your Email",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Verify Email</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:#050505;line-height:1px;font-size:1px;"">
    Verify your email to activate your account.
  </div>

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
              <h2 style=""margin:0 0 14px 0;font-size:20px;font-weight:900;"">Verify Your Email</h2>

              <p style=""margin:0 0 16px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Hello <span style=""color:#ffffff;font-weight:800;"">{{CustomerName}}</span>,
              </p>

              <p style=""margin:0 0 18px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Click the button below to verify your email and activate your account.
              </p>

              <div style=""text-align:center;padding:10px 0 4px 0;"">
                <a href=""{{VerifyUrl}}"" style=""display:inline-block;padding:14px 30px;background:#edbc1d;color:#000000;text-decoration:none;border-radius:10px;font-weight:900;font-size:12px;text-transform:uppercase;letter-spacing:1px;"">
                  Verify Email
                </a>
              </div>

              <p style=""margin:14px 0 0 0;color:#888888;font-size:12px;line-height:1.6;"">
                Or open: <a href=""{{VerifyUrl}}"" style=""color:#edbc1d;text-decoration:underline;word-break:break-word;"">{{VerifyUrl}}</a>
              </p>
            </td>
          </tr>

          <tr>
            <td style=""background:#0a0a0a;padding:18px 22px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems • All rights reserved
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
            
            // 10) INT-CAR-UPD
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "INT-CAR-UPD",
                    "Car Updated (Internal)",
                    "Internal",
                    "Fleet Update",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Fleet Update</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:#050505;line-height:1px;font-size:1px;"">
    Internal fleet update log.
  </div>

  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#050505;padding:40px 16px;font-family:Manrope,Segoe UI,Arial,sans-serif;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:720px;background:#121212;border:1px solid #2a2a2a;border-radius:16px;overflow:hidden;"">
          <tr>
            <td style=""background:#0a0a0a;padding:22px 24px;border-bottom:1px solid #2a2a2a;"">
              <div style=""font-size:18px;font-weight:900;letter-spacing:1.2px;text-transform:uppercase;color:#edbc1d;"">RentLuxury • Internal</div>
              <div style=""margin-top:6px;color:#888888;font-size:12px;"">Fleet Update Log</div>
            </td>
          </tr>

          <tr>
            <td style=""padding:24px;color:#ffffff;"">
              <p style=""margin:0 0 12px 0;color:#a3a3a3;font-size:14px;line-height:1.7;"">
                Vehicle configuration updated by <span style=""color:#ffffff;font-weight:900;"">{{UpdatedBy}}</span>.
              </p>

              <div style=""background:#0a0a0a;border:1px solid #2a2a2a;border-radius:12px;padding:16px;
                          font-family:ui-monospace,SFMono-Regular,Menlo,Monaco,Consolas,'Liberation Mono','Courier New',monospace;
                          font-size:12px;color:#a3a3a3;line-height:1.7;"">
                Vehicle: <span style=""color:#ffffff;font-weight:700;"">{{CarModel}}</span> ({{PlateNumber}})<br><br>
                CHANGELOG:<br>
                {{ChangeSummary}}
              </div>
            </td>
          </tr>

          <tr>
            <td style=""background:#0a0a0a;padding:14px 18px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems
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
            
            // 11) INT-CAT-UPD
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "INT-CAT-UPD",
                    "Category Pricing Updated (Internal)",
                    "Internal",
                    "Pricing Update",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Pricing Update</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:#050505;line-height:1px;font-size:1px;"">
    Internal pricing update log.
  </div>

  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#050505;padding:40px 16px;font-family:Manrope,Segoe UI,Arial,sans-serif;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:720px;background:#121212;border:1px solid #2a2a2a;border-radius:16px;overflow:hidden;"">
          <tr>
            <td style=""background:#0a0a0a;padding:22px 24px;border-bottom:1px solid #2a2a2a;"">
              <div style=""font-size:18px;font-weight:900;letter-spacing:1.2px;text-transform:uppercase;color:#edbc1d;"">RentLuxury • Internal</div>
              <div style=""margin-top:6px;color:#888888;font-size:12px;"">Pricing Update Log</div>
            </td>
          </tr>

          <tr>
            <td style=""padding:24px;color:#ffffff;"">
              <p style=""margin:0 0 14px 0;color:#a3a3a3;font-size:14px;line-height:1.7;"">
                Rental category pricing adjustment recorded:
              </p>

              <div style=""background:#0a0a0a;border:1px solid #2a2a2a;border-radius:12px;padding:16px;"">
                <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">Category</div>
                <div style=""color:#ffffff;font-weight:900;margin:4px 0 12px 0;"">{{CategoryName}}</div>

                <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">New Price</div>
                <div style=""color:#edbc1d;font-weight:900;margin-top:4px;"">{{NewPrice}}</div>
              </div>
            </td>
          </tr>

          <tr>
            <td style=""background:#0a0a0a;padding:14px 18px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems
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
            
            // 12) INT-PROMO-EXP
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "INT-PROMO-EXP",
                    "Promo Expiry Alert (Internal)",
                    "Internal",
                    "Promo Expiry Alert",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Promo Expiry Alert</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:#050505;line-height:1px;font-size:1px;"">
    Internal promo expiry alert.
  </div>

  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#050505;padding:40px 16px;font-family:Manrope,Segoe UI,Arial,sans-serif;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:720px;background:#121212;border:1px solid #2a2a2a;border-radius:16px;overflow:hidden;"">
          <tr>
            <td style=""background:#0a0a0a;padding:22px 24px;border-bottom:1px solid #2a2a2a;"">
              <div style=""font-size:18px;font-weight:900;letter-spacing:1.2px;text-transform:uppercase;color:#edbc1d;"">RentLuxury • Internal</div>
              <div style=""margin-top:6px;color:#888888;font-size:12px;"">System Alert</div>
            </td>
          </tr>

          <tr>
            <td style=""padding:24px;color:#ffffff;"">
              <h3 style=""margin:0 0 12px 0;font-size:16px;font-weight:900;"">Promo Expiry</h3>

              <p style=""margin:0 0 14px 0;color:#a3a3a3;font-size:14px;line-height:1.7;"">
                The promotion <span style=""color:#ffffff;font-weight:900;"">{{PromoCode}}</span> is scheduled to expire.
              </p>

              <div style=""background:#0a0a0a;border:1px solid #2a2a2a;border-radius:12px;padding:16px;"">
                <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">Expiry Date</div>
                <div style=""color:#edbc1d;font-weight:900;margin:4px 0 12px 0;"">{{ExpiryDate}}</div>

                <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">Usage</div>
                <div style=""color:#ffffff;font-weight:900;margin-top:4px;"">{{UsageCount}} redemptions</div>
              </div>
            </td>
          </tr>

          <tr>
            <td style=""background:#0a0a0a;padding:14px 18px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems
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


            // 13) INT-PROMOC-UPD
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "INT-PROMOC-UPD",
                    "Promocode Modified (Internal)",
                    "Internal",
                    "Promo Modified",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Promo Modified</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:#050505;line-height:1px;font-size:1px;"">
    Internal promo configuration changed.
  </div>

  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#050505;padding:40px 16px;font-family:Manrope,Segoe UI,Arial,sans-serif;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:720px;background:#121212;border:1px solid #2a2a2a;border-radius:16px;overflow:hidden;"">
          <tr>
            <td style=""background:#0a0a0a;padding:22px 24px;border-bottom:1px solid #2a2a2a;"">
              <div style=""font-size:18px;font-weight:900;letter-spacing:1.2px;text-transform:uppercase;color:#edbc1d;"">RentLuxury • Internal</div>
              <div style=""margin-top:6px;color:#888888;font-size:12px;"">Promo Log</div>
            </td>
          </tr>

          <tr>
            <td style=""padding:24px;color:#ffffff;"">
              <p style=""margin:0 0 14px 0;color:#a3a3a3;font-size:14px;line-height:1.7;"">
                Promotion <span style=""color:#ffffff;font-weight:900;"">{{PromoCode}}</span> configuration changed.
              </p>

              <div style=""background:#0a0a0a;border:1px solid #2a2a2a;border-radius:12px;padding:16px;
                          font-family:ui-monospace,SFMono-Regular,Menlo,Monaco,Consolas,'Liberation Mono','Courier New',monospace;
                          font-size:12px;color:#a3a3a3;line-height:1.7;"">
                {{ChangeDetail}}
              </div>
            </td>
          </tr>

          <tr>
            <td style=""background:#0a0a0a;padding:14px 18px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems
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
            
            // 14) INT-DOCS-UNV
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "INT-DOCS-UNV",
                    "Unverified Docs Report (Internal)",
                    "Internal",
                    "Compliance Queue",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Compliance Queue</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:#050505;line-height:1px;font-size:1px;"">
    Daily compliance queue report.
  </div>

  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#050505;padding:40px 16px;font-family:Manrope,Segoe UI,Arial,sans-serif;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:720px;background:#121212;border:1px solid #2a2a2a;border-radius:16px;overflow:hidden;"">
          <tr>
            <td style=""background:#0a0a0a;padding:22px 24px;border-bottom:1px solid #2a2a2a;"">
              <div style=""font-size:18px;font-weight:900;letter-spacing:1.2px;text-transform:uppercase;color:#edbc1d;"">RentLuxury • Internal</div>
              <div style=""margin-top:6px;color:#888888;font-size:12px;"">Compliance Report</div>
            </td>
          </tr>

          <tr>
            <td style=""padding:24px;color:#ffffff;"">
              <h3 style=""margin:0 0 10px 0;font-size:16px;font-weight:900;"">Pending Verification Queue</h3>

              <div style=""background:#0a0a0a;border:1px solid rgba(237,188,29,0.25);border-radius:12px;padding:18px;margin:14px 0;text-align:center;"">
                <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">Count</div>
                <div style=""margin-top:8px;color:#edbc1d;font-size:34px;font-weight:900;"">{{Count}}</div>
              </div>

              <p style=""margin:0;color:#a3a3a3;font-size:14px;line-height:1.7;"">
                Users pending for more than 24 hours.
              </p>
            </td>
          </tr>

          <tr>
            <td style=""background:#0a0a0a;padding:14px 18px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems
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
            
            // 15) REM-PAY-GENERIC
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "REM-PAY-GENERIC",
                    "Payment Reminder",
                    "Reminder",
                    "Payment Reminder",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Payment Reminder</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:#050505;line-height:1px;font-size:1px;"">
    Payment reminder for your booking.
  </div>

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
              <h2 style=""margin:0 0 14px 0;font-size:20px;font-weight:900;"">Payment Reminder</h2>

              <p style=""margin:0 0 16px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Hello <span style=""color:#ffffff;font-weight:800;"">{{CustomerName}}</span>,
              </p>

              <p style=""margin:0 0 16px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                This is a reminder for the outstanding balance on Booking <span style=""color:#ffffff;font-weight:900;"">#{{BookingId}}</span>.
              </p>

              <div style=""background:#0a0a0a;border:1px solid rgba(237,188,29,0.25);border-radius:12px;padding:18px;margin:18px 0;text-align:center;"">
                <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">Outstanding</div>
                <div style=""margin-top:8px;color:#edbc1d;font-size:26px;font-weight:900;"">{{Amount}}</div>
              </div>

              <div style=""text-align:center;padding:10px 0 4px 0;"">
                <a href=""{{PaymentUrl}}"" style=""display:inline-block;padding:14px 30px;background:#edbc1d;color:#000000;text-decoration:none;border-radius:10px;font-weight:900;font-size:12px;text-transform:uppercase;letter-spacing:1px;"">
                  Pay Now
                </a>
              </div>

              <p style=""margin:14px 0 0 0;color:#888888;font-size:12px;line-height:1.6;"">
                Or open: <a href=""{{PaymentUrl}}"" style=""color:#edbc1d;text-decoration:underline;word-break:break-word;"">{{PaymentUrl}}</a>
              </p>
            </td>
          </tr>

          <tr>
            <td style=""background:#0a0a0a;padding:18px 22px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems • All rights reserved
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
            
            // 16) REM-PICK-INSTR
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "REM-PICK-INSTR",
                    "Pickup Instructions",
                    "Reminder",
                    "Pickup Instructions",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Pickup Instructions</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:#050505;line-height:1px;font-size:1px;"">
    Pickup details for your trip.
  </div>

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
              <h2 style=""margin:0 0 14px 0;font-size:20px;font-weight:900;"">Trip Ready</h2>

              <p style=""margin:0 0 16px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Hello <span style=""color:#ffffff;font-weight:800;"">{{CustomerName}}</span>,
              </p>

              <p style=""margin:0 0 18px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Here are your pickup details:
              </p>

              <div style=""background:#0a0a0a;border:1px solid #2a2a2a;border-radius:12px;padding:18px;margin:18px 0;"">
                <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">Vehicle</div>
                <div style=""color:#ffffff;font-weight:900;margin:4px 0 12px 0;"">{{CarModel}}</div>

                <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">Location</div>
                <div style=""color:#edbc1d;font-weight:900;margin:4px 0 12px 0;"">{{PickupLocation}}</div>

                <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">Time</div>
                <div style=""color:#ffffff;font-weight:900;margin-top:4px;"">{{PickupTime}}</div>
              </div>

              <p style=""margin:0;color:#888888;font-size:12px;line-height:1.6;"">
                Please bring your physical driver’s license and ID.
              </p>
            </td>
          </tr>

          <tr>
            <td style=""background:#0a0a0a;padding:18px 22px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems • All rights reserved
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
            
            // 17) REM-RET-INSTR
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "TemplateKey", "Name", "Category", "Subject", "Body", "IsActive", "UpdatedAt" },
                values: new object[] {
                    "REM-RET-INSTR",
                    "Return Instructions",
                    "Reminder",
                    "Return Instructions",
                    @"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Return Instructions</title>
</head>
<body style=""margin:0;padding:0;background:#050505;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:#050505;line-height:1px;font-size:1px;"">
    Return instructions for your rental.
  </div>

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
              <h2 style=""margin:0 0 14px 0;font-size:20px;font-weight:900;"">Return Instructions</h2>

              <p style=""margin:0 0 16px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Hello <span style=""color:#ffffff;font-weight:800;"">{{CustomerName}}</span>,
              </p>

              <p style=""margin:0 0 12px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                We hope you’re enjoying your experience with <span style=""color:#ffffff;font-weight:900;"">{{CarModel}}</span>.
              </p>

              <p style=""margin:0 0 18px 0;color:#a3a3a3;font-size:14px;line-height:1.75;"">
                Your rental concludes on <span style=""color:#edbc1d;font-weight:900;"">{{ReturnDate}}</span>.
              </p>

              <div style=""background:#0a0a0a;border:1px solid #2a2a2a;border-left:3px solid #edbc1d;border-radius:12px;padding:16px;margin:18px 0;"">
                <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">Return Location</div>
                <div style=""color:#ffffff;font-weight:900;margin-top:6px;"">{{ReturnLocation}}</div>
              </div>

              <p style=""margin:0;color:#888888;font-size:12px;line-height:1.6;"">
                Thank you for choosing RentLuxury.
              </p>
            </td>
          </tr>

          <tr>
            <td style=""background:#0a0a0a;padding:18px 22px;text-align:center;border-top:1px solid #2a2a2a;"">
              <div style=""color:#888888;font-size:11px;letter-spacing:1px;text-transform:uppercase;"">
                &copy; {{Year}} RentLuxury Systems • All rights reserved
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.DeleteData(
                table: "EmailTemplates",
                keyColumn: "TemplateKey",
                keyValues: new object[] {
                    "AUTH-ACC-STATUS", "AUTH-V2-VERIFY", "AUTH-RESET-V1", "AUTH-OTP-SECURE", "AUTH-PWD-CHANGE",
                    "CUST-BOOK-UPDATE", "CUST-DOC-VERIFY", "CUST-PAY-FAILED", "AUTH-VERIFY-EMAIL",
                    "INT-CAR-UPD", "INT-CAT-UPD", "INT-PROMO-EXP", "INT-PROMOC-UPD", "INT-DOCS-UNV",
                    "REM-PAY-GENERIC", "REM-PICK-INSTR", "REM-RET-INSTR"
                });
        }
    }
}
