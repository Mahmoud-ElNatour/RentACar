using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities;

public partial class Booking
{
    [Key]
    [Column("BookingID")]
    public int BookingId { get; set; }

    [Column("customerID")]
    public int CustomerId { get; set; }

    [Column("carID")]
    public int CarId { get; set; }

    [Column("isBookedByEmployee")]
    public bool? IsBookedByEmployee { get; set; }

    [Column("EmployeebookerID")]
    public int? EmployeebookerId { get; set; }

    [Column("startdate")]
    public DateOnly Startdate { get; set; }

    [Column("enddate")]
    public DateOnly Enddate { get; set; }

    [Column("promocodeID")]
    public int? PromocodeId { get; set; }

    [Column("totalPrice", TypeName = "decimal(18, 2)")]
    public decimal TotalPrice { get; set; }

    [Column("hasDriver")]
    public bool HasDriver { get; set; }

    [Column("driverID")]
    public int? DriverId { get; set; }

    [Column("driverDailyFee", TypeName = "decimal(18, 2)")]
    public decimal? DriverDailyFee { get; set; }

    [Column("pickupAddress")]
    [StringLength(200)]
    public string? PickupAddress { get; set; }

    [Column("pickupLocationLabel")]
    [StringLength(200)]
    public string? PickupLocationLabel { get; set; }

    [Column("pickupDateTime")]
    public DateTime? PickupDateTime { get; set; }

    [Column("bookingStatus")]
    [StringLength(50)]
    public string? BookingStatus { get; set; }

    [Column("subtotal", TypeName = "decimal(18, 2)")]
    public decimal? Subtotal { get; set; }

    [ForeignKey("CarId")]
    [InverseProperty("Bookings")]
    public virtual Car Car { get; set; } = null!;
    [Column("pickupLatitude")]
    public double? PickupLatitude { get; set; }
    [Column("pickupLongitude")]
    public double? PickupLongitude { get; set; }


    [ForeignKey("CustomerId")]
    [InverseProperty("Bookings")]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("DriverId")]
    [InverseProperty("Bookings")]
    public virtual Driver? Driver { get; set; }

    [ForeignKey("EmployeebookerId")]
    [InverseProperty("Bookings")]
    public virtual Employee? Employeebooker { get; set; }

    [ForeignKey("PromocodeId")]
    [InverseProperty("Bookings")]
    public virtual Promocode? Promocode { get; set; }

    [InverseProperty("Booking")]
    public virtual ICollection<DriverLocationPing> DriverLocationPings { get; set; } = new List<DriverLocationPing>();

    [InverseProperty("Booking")]
    public virtual Trip? Trip { get; set; }

    public virtual Payment? Payment { get; set; }
}
