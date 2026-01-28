using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RentACar.Core.Entities;

public partial class Category
{
    [Key]
    [Column("categoryID")]
    public int CategoryId { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column("image")]
    public byte[]? Image { get; set; }

    public bool IsActive { get; set; } = true;

    [InverseProperty("Category")]
    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
}
