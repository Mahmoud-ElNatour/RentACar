using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities;

[Table("CustomerCreditCard")]
public class CustomerCreditCard
{
    public int UserId { get; set; }

    public int CreditCardId { get; set; }

    [ForeignKey("CreditCardId")]
    [InverseProperty("CustomerCreditCards")]
    public virtual CreditCard CreditCard { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("CustomerCreditCards")]
    public virtual Customer User { get; set; } = null!;
}
