using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities;

[Table("PaymentMethods")]
public partial class PaymentMethod
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("paymentMethodName")]
    [Required]
    [MaxLength(50)]
    public string PaymentMethodName { get; set; } = null!;
}
