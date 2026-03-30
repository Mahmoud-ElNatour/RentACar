using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RentACar.Core.Enums;

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

    [Column("extraDriverFeePerDay", TypeName = "decimal(10, 2)")]
    public decimal? ExtraDriverFeePerDay { get; set; }
    [Column("isAvailable")]
    public bool IsAvailable { get; set; }

    [Column("categoryID")]
    public int? CategoryId { get; set; }

    [Column("carImage")]
    public byte[]? CarImage { get; set; }

    [Column("seatsCapacity")]
    public int SeatsCapacity { get; set; }

    [Column("supportsBabySeat")]
    public bool SupportsBabySeat { get; set; }

    [Column("doors")]
    public int Doors { get; set; }

    [Column("transmissionType")]
    public TransmissionType TransmissionType { get; set; }

    [Column("fuelType")]
    public FuelType FuelType { get; set; }

    [Column("luggageCapacity")]
    public int? LuggageCapacity { get; set; }

    [Column("hasInfotainmentScreen")]
    public bool HasInfotainmentScreen { get; set; }

    [Column("hasGPS")]
    public bool HasGPS { get; set; }

    [Column("hasSunroof")]
    public bool HasSunroof { get; set; }
 
    [Column("hasParkingSensors")]
    public bool HasParkingSensors { get; set; }

    [Column("hasRearCamera")]
    public bool HasRearCamera { get; set; }

    [InverseProperty("Car")]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    [ForeignKey("CategoryId")]
    [InverseProperty("Cars")]
    public virtual Category? Category { get; set; }
}
