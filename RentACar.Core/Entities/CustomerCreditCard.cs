using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities;

[Table("CustomerCreditCard")]
public class CustomerCreditCard
{
    [Key]
    [Column("userId", Order = 0)]
    public int UserId { get; set; }

    [Key]
    [Column("creditCardId", Order = 1)]
    public int CreditCardId { get; set; }

    [ForeignKey("CreditCardId")]
    [InverseProperty("CustomerCreditCards")]
    public virtual CreditCard CreditCard { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("CustomerCreditCards")]
    public virtual Customer User { get; set; } = null!;
}
