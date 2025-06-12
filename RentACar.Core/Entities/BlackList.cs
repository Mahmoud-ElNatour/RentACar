using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RentACar.Core.Entities;

[Table("BlackList")]
public partial class BlackList
{
    [Column("userID")]
    public String UserId { get; set; } = null!;

    [Column("reason")]
    public string? Reason { get; set; }

    [Column("dateBlocked")]
    public DateOnly DateBlocked { get; set; }

    [Key]
    [Column("blacklistID")]
    public int BlacklistId { get; set; }

    [Column("employeeDoneBlacklistId")]
    
    public int EmployeeDoneBlacklistId { get; set; } = 0!;

    [ForeignKey("EmployeeDoneBlacklistId")]
    [InverseProperty("BlackLists")]
    public virtual Employee EmployeeDoneBlacklist { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("BlackLists")]
    public virtual AspNetUser User { get; set; } = null!;
}
