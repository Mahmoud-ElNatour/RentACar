using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities;

public partial class ExpenseCategory
{
    [Key]
    public int ExpenseCategoryId { get; set; }

    [Required]
    [StringLength(80)]
    public string Name { get; set; } = null!;

    [StringLength(200)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [InverseProperty("ExpenseCategory")]
    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
