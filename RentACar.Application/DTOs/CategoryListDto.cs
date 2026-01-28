using System;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs
{
    public class CategoryListDto
    {
        public int CategoryId { get; set; }

        public string Name { get; set; } = null!;

        public int CarsCount { get; set; }

        public bool IsActive { get; set; }
    }
}
