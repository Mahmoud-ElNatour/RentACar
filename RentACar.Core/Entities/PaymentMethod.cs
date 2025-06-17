using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities;

public partial class PaymentMethod
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("PaymentMethodName")]
    [StringLength(50)]
    public string PaymentMethodName { get; set; } = null!;
}
