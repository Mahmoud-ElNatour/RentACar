using System;

namespace RentACar.Application.DTOs
{
    public class EmailDraftDto
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string RecipientsRaw { get; set; }
        public string SelectedDistributionListIdsRaw { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class SaveDraftRequestDto
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string RecipientsRaw { get; set; }
        public string SelectedDistributionListIdsRaw { get; set; }
    }

    public class SendEmailRequestDto
    {
         public string RecipientsRaw { get; set; } // Comma separated emails
         public string SelectedDistributionListIdsRaw { get; set; } // Comma separated IDs
         public string Subject { get; set; }
         public string Body { get; set; }
         public bool IsTemplateMode { get; set; }
         public string TemplateKey { get; set; }
         public System.Collections.Generic.List<Microsoft.AspNetCore.Http.IFormFile> Attachments { get; set; }
         
         // In "Template Mode", Body might be auto-generated or overridden.
         // If IsTemplateMode is true, we might use TemplateKey to look up the base template, 
         // but if the user EDITED it in the composer, 'Body' should contain the final HTML.
         // Wait, the HTML template shows "Save as New Template" and "Send Message".
         // If they edit the body in the composer, we should probably send THAT body.
    }
}
