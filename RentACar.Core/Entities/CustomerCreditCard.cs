using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RentACar.Core.Entities;

[Table("CustomerCreditCard")]
public partial class CustomerCreditCard
{
    [Key]
    public int CustomerCreditCardId { get; set; }

    [Column("userId")]
   
    public int UserId { get; set; } = 0!;

    [Column("creditCardId")]
    public int CreditCardId { get; set; }

    [ForeignKey("CreditCardId")]
    [InverseProperty("CustomerCreditCards")]
    public virtual CreditCard CreditCard { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("CustomerCreditCards")]
    public virtual Customer User { get; set; } = null!;
}
