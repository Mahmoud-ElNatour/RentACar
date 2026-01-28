using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentACar.Application.DTOs
{
    public class BrowseViewDTO
    {
        public IEnumerable<CarListDto> Cars { get; set; } = new List<CarListDto>();
        public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public bool HasMore => CurrentPage < TotalPages;

        // To keep filter inputs sticky on the form
        public string? FilterName { get; set; }
        public IEnumerable<int> FilterCategoryIds { get; set; } = new List<int>();
        public decimal? FilterMinPrice { get; set; }
        public decimal? FilterMaxPrice { get; set; }

        public DateOnly? FilterStartDate { get; set; }
        public DateOnly? FilterEndDate { get; set; }
    }
}

