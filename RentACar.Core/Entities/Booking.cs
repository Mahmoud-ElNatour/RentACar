using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RentACar.Core.Entities;

public partial class Booking
{
    [Key]
    [Column("BookingID")]
    public int BookingId { get; set; }

    [Column("customerID")]
  
    public int CustomerId { get; set; } = 0!;

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

    [Column("bookingStatus")]
    [StringLength(50)]
    public string? BookingStatus { get; set; }

    [Column("paymentID")]
    public int? PaymentId { get; set; }

    [Column("subtotal", TypeName = "decimal(18, 2)")]
    public decimal? Subtotal { get; set; }

    [ForeignKey("CarId")]
    [InverseProperty("Bookings")]
    public virtual Car Car { get; set; } = null!;

    [ForeignKey("CustomerId")]
    [InverseProperty("Bookings")]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("EmployeebookerId")]
    [InverseProperty("Bookings")]
    public virtual Employee? Employeebooker { get; set; }

    [ForeignKey("PaymentId")]
    [InverseProperty("Bookings")]
    public virtual Payment? Payment { get; set; }

    [InverseProperty("Booking")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [ForeignKey("PromocodeId")]
    [InverseProperty("Bookings")]
    public virtual Promocode? Promocode { get; set; }
}
