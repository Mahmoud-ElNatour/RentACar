using System;
using System.Collections.Generic;
using RentACar.Core.Entities;

namespace RentACar.Web.Areas.Admin.ViewModels.EmailServices
{
    public class NotificationLogListVM
    {
        public List<NotificationLog> Logs { get; set; } = new List<NotificationLog>();
        
        // Filters
        public string EventType { get; set; }
        public string Result { get; set; }
        public string SearchTerm { get; set; } // Booking ID or Recipient
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        
        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
