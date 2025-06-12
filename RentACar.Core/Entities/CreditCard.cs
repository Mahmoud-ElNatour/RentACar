using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RentACar.Core.Entities;

[Table("CreditCard")]
public partial class CreditCard
{
    [Key]
    [Column("creditCardID")]
    public int CreditCardId { get; set; }

    [Column("cardNumber")]
    [StringLength(50)]
    [Unicode(false)]
    public string CardNumber { get; set; } = null!;

    [Column("cardHolderName")]
    [StringLength(50)]
    public string CardHolderName { get; set; } = null!;

    [Column("expiryDate")]
    public DateOnly ExpiryDate { get; set; }

    [Column("cvv")]
    [StringLength(10)]
    public string Cvv { get; set; } = null!;

    [InverseProperty("CreditCard")]
    public virtual ICollection<CustomerCreditCard> CustomerCreditCards { get; set; } = new List<CustomerCreditCard>();
}
