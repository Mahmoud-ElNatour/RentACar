using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RentACar.Core.Entities;

public partial class Payment
{
    [Key]
    [Column("paymentID")]
    public int PaymentId { get; set; }

    [Column("bookingID")]
    public int BookingId { get; set; }

    [Column("amount", TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [Column("paymentDate")]
    public DateOnly PaymentDate { get; set; }

    [Column("creditcardID")]
    public int? CreditcardId { get; set; }

    [Column("paymentMethod")]
    [StringLength(20)]
    [Unicode(false)]
    public string? PaymentMethod { get; set; }

    [Column("status")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    [ForeignKey("BookingId")]
    [InverseProperty("Payments")]
    public virtual Booking Booking { get; set; } = null!;

    [InverseProperty("Payment")]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
