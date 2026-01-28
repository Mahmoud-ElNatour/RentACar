using System;

namespace RentACar.Application.Managers
{
    public static class EmailTemplates
    {
        public static string GetStandardTemplate(string content, string title = "Notification")
        {
            // RentACar Professional Template
            // Theme: Dark & Gold
            string primaryColor = "#d4af37"; // Gold
            string backgroundColor = "#1a1a1a"; // Dark Gray
            string cardColor = "#2d2d2d"; // Slightly lighter gray for card
            string textColor = "#ffffff";
            string mutedColor = "#aaaaaa";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
    <style>
        body {{ margin: 0; padding: 0; background-color: {backgroundColor}; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }}
        table {{ border-spacing: 0; }}
        td {{ padding: 0; }}
        img {{ border: 0; }}
        .wrapper {{ width: 100%; table-layout: fixed; background-color: {backgroundColor}; padding-bottom: 40px; }}
        .webkit {{ max-width: 600px; background-color: {backgroundColor}; }}
        .outer {{ margin: 0 auto; width: 100%; max-width: 600px; border-spacing: 0; font-family: sans-serif; color: {textColor}; }}
        
        .header h1 {{ margin: 0; color: {primaryColor}; font-size: 28px; letter-spacing: 2px; text-transform: uppercase; text-align: center; padding-top: 20px; padding-bottom: 20px; }}
        .header span {{ color: {textColor}; }}
        
        .card {{ background-color: {cardColor}; border-radius: 8px; padding: 40px; box-shadow: 0 4px 15px rgba(0,0,0,0.3); border-top: 4px solid {primaryColor}; }}
        .content {{ color: {textColor}; line-height: 1.6; font-size: 16px; }}
        .content h2 {{ color: {primaryColor}; margin-top: 0; }}
        
        .btn {{ display: inline-block; padding: 12px 24px; background-color: {primaryColor}; color: #000; text-decoration: none; border-radius: 4px; font-weight: bold; margin-top: 20px; }}
        .footer {{ text-align: center; padding-top: 30px; color: {mutedColor}; font-size: 12px; }}
        .footer a {{ color: {primaryColor}; text-decoration: none; }}
    </style>
</head>
<body style='margin: 0; padding: 0; background-color: {backgroundColor};'>
    <center class='wrapper' style='width: 100%; table-layout: fixed; background-color: {backgroundColor}; padding-bottom: 40px;'>
        <div class='webkit' style='max-width: 600px; background-color: {backgroundColor};'>
            <table class='outer' align='center' style='margin: 0 auto; width: 100%; max-width: 600px; border-spacing: 0; font-family: sans-serif; color: {textColor};'>
                <!-- Header -->
                <tr>
                    <td style='padding: 20px 0; text-align: center;'>
                        <div class='header'>
                             <h1 style='margin: 0; color: {primaryColor}; font-size: 28px; letter-spacing: 2px; text-transform: uppercase;'>Rent<span style='color: {textColor};'>ACar</span></h1>
                        </div>
                    </td>
                </tr>
                
                <!-- Card -->
                <tr>
                    <td style='padding: 0;'>
                        <div class='card' style='background-color: {cardColor}; border-radius: 8px; padding: 40px; box-shadow: 0 4px 15px rgba(0,0,0,0.3); border-top: 4px solid {primaryColor};'>
                            <div class='content' style='color: {textColor}; line-height: 1.6; font-size: 16px;'>
                                {content}
                            </div>
                        </div>
                    </td>
                </tr>

                <!-- Footer -->
                <tr>
                    <td style='padding-top: 30px; text-align: center; color: {mutedColor}; font-size: 12px;'>
                         <p style='margin: 5px 0;'>&copy; {DateTime.Now.Year} RentACar. All rights reserved.</p>
                         <p style='margin: 5px 0;'>You received this email because you have an account with us.</p>
                    </td>
                </tr>
            </table>
        </div>
    </center>
</body>
</html>";
        }
    }
}
