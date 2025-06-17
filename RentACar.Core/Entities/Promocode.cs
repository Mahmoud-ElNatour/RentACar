    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;

    namespace RentACar.Core.Entities;

    public partial class Promocode
    {
        [Key]
        [Column("promocodeID")]
        public int PromocodeId { get; set; }

        [Column("discountPercentage", TypeName = "decimal(18, 2)")]
        public decimal DiscountPercentage { get; set; }

        [Column("validUntil")]
        public DateOnly? ValidUntil { get; set; }

        [StringLength(50)]
        public string? Description { get; set; }
        [Column("isActive")]
        public bool IsActive { get; set; }

        [Column("name")]
        [StringLength(50)]
        public string? Name { get; set; }

        [InverseProperty("Promocode")]
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
