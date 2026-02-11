using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities;

public partial class Expense
{
    [Key]
    public int ExpenseId { get; set; }

    [Required]
    public int ExpenseCategoryId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [Required]
    public DateOnly ExpenseDate { get; set; }

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = null!; // Planned, Paid, Cancelled

    [Required]
    [StringLength(120)]
    public string Title { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(120)]
    public string? Vendor { get; set; }

    [StringLength(60)]
    public string? ReferenceNumber { get; set; }

    [StringLength(450)]
    public string? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("ExpenseCategoryId")]
    [InverseProperty("Expenses")]
    public virtual ExpenseCategory ExpenseCategory { get; set; } = null!;

    [ForeignKey("CreatedByUserId")]
    public virtual AspNetUser? CreatedByUser { get; set; }
}
