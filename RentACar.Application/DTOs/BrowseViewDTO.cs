using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentACar.Application.DTOs
{
    public class BrowseViewDTO
    {
        public IEnumerable<CarDto> Cars { get; set; } = new List<CarDto>();
        public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();

        // To keep filter inputs sticky on the form
        public string? FilterName { get; set; }
        public int? FilterCategoryId { get; set; }
        public decimal? FilterMaxPrice { get; set; }

        public DateOnly? FilterStartDate { get; set; }
        public DateOnly? FilterEndDate { get; set; }
    }
}

