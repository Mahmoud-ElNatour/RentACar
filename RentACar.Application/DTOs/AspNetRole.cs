﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RentACar.Application.DTOs
{

    public partial class AspNetRole
    {
        [Key]
        public string Id { get; set; } = null!;

        [StringLength(256)]
        public string? Name { get; set; }

        [StringLength(256)]
        public string? NormalizedName { get; set; }

        public string? ConcurrencyStamp { get; set; }

        [InverseProperty("Role")]
        public virtual ICollection<AspNetRoleClaim> AspNetRoleClaims { get; set; } = new List<AspNetRoleClaim>();

        [ForeignKey("RoleId")]
        [InverseProperty("Roles")]
        public virtual ICollection<AspNetUser> Users { get; set; } = new List<AspNetUser>();
    }


    public class ChangeRoleDTO
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;
    }

}