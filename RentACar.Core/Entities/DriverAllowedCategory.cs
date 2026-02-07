using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities;

[Table("DriverAllowedCategories")]
public class DriverAllowedCategory
{
    [Key]
    public int Id { get; set; }

    [Column("driverID")]
    public int DriverId { get; set; }

    [Column("categoryID")]
    public int CategoryId { get; set; }

    [ForeignKey("DriverId")]
    [InverseProperty("AllowedCategories")]
    public virtual Driver Driver { get; set; } = null!;

    [ForeignKey("CategoryId")]
    [InverseProperty("DriverAllowedCategories")]
    public virtual Category Category { get; set; } = null!;
}
