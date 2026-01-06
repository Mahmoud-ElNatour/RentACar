using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RentACar.Application.DTOs
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        // For Display
        public string? ImageBase64 { get; set; }

        // For Upload
        [System.Text.Json.Serialization.JsonIgnore] 
        public Microsoft.AspNetCore.Http.IFormFile? ImageFile { get; set; }

        public int CarsCount { get; set; }

        public bool IsActive { get; set; }
    }
}
