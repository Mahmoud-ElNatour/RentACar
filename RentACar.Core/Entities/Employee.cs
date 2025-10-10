using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RentACar.Core.Entities;

public partial class Employee
{
    [Key]
    [Column("employeeID")]
    public int EmployeeId { get; set; } = 0!;

    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column("aspNetUserId")]
    [StringLength(450)]
    public string aspNetUserId { get; set; } = null!;

    [Column("salary", TypeName = "decimal(18, 2)")]
    public decimal? Salary { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("isActive")]
    public bool IsActive { get; set; }

    // Foreign Key to AspNetUser
    [ForeignKey("aspNetUserId")] // EmployeeId in this table maps to the Id in AspNetUser
    public virtual AspNetUser User { get; set; } = null!; // Navigation property to the AspNetUser

    [InverseProperty("EmployeeDoneBlacklist")]
    public virtual ICollection<BlackList> BlackLists { get; set; } = new List<BlackList>();

    [InverseProperty("Employeebooker")]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}