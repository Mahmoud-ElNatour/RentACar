using System;
using System.ComponentModel.DataAnnotations;

namespace RentACar.Core.Entities
{
    public class EmailTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TemplateKey { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        // Customer, Employee, System
        public string Category { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserId { get; set; }
    }
}
