using System;
using System.Collections.Generic;
using RentACar.Core.Entities;

namespace RentACar.Application.DTOs
{
    public class AuditLogViewModel
    {
        public List<AuditLogDto> Logs { get; set; } = new();
        
        // Filter properties
        public string? SearchTerm { get; set; }
        public string? ActionType { get; set; }
        public string? EntityName { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        // Dynamic Filter Data
        public List<string> AvailableActions { get; set; } = new();
        public List<string> AvailableEntities { get; set; } = new();

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }
}
