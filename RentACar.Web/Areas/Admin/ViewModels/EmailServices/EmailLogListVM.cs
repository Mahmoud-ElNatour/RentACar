using System;
using System.Collections.Generic;
using RentACar.Core.Entities;

namespace RentACar.Web.Areas.Admin.ViewModels.EmailServices
{
    public class EmailLogListVM
    {
        public List<EmailLog> Logs { get; set; } = new List<EmailLog>();
        
        // Filters
        public string Status { get; set; }
        public string EmailType { get; set; }
        public string SearchTerm { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        
        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
