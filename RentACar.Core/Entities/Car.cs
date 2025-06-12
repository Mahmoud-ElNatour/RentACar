using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RentACar.Core.Entities;

public partial class Car
{
    [Key]
    [Column("CarID")]
    public int CarId { get; set; }

    [Column("plateNumber")]
    [StringLength(50)]
    public string PlateNumber { get; set; } = null!;

    [Column("modelName")]
    [StringLength(50)]
    public string ModelName { get; set; } = null!;

    [Column("modelYear")]
    public int ModelYear { get; set; }

    [Column("color")]
    [StringLength(50)]
    public string? Color { get; set; }

    [Column("pricePerDay", TypeName = "decimal(18, 2)")]
    public decimal? PricePerDay { get; set; }
    [Column("isAvailable")]
    public bool IsAvailable { get; set; }

    [Column("categoryID")]
    public int? CategoryId { get; set; }

    [InverseProperty("Car")]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    [ForeignKey("CategoryId")]
    [InverseProperty("Cars")]
    public virtual Category? Category { get; set; }
}
