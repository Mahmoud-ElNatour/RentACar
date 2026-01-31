using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

<<<<<<< HEAD

=======
>>>>>>> Mahmoud-V3
namespace RentACar.Core.Entities;

public partial class Customer
{
<<<<<<< HEAD
    private ICollection<CustomerRating> customerRatings = new List<CustomerRating>();

=======
>>>>>>> Mahmoud-V3
    [Key]
    [Column("userID")]
    public int UserId { get; set; } = 0!;

    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;


    [Column("aspNetUserId")]
    [StringLength(450)]
    public string aspNetUserId { get; set; } = null!;

    [Column("isVerified")]
    public bool IsVerified { get; set; }

    [Column("drivingLicenseFront", TypeName = "image")]
    public byte[]? DrivingLicenseFront { get; set; }

    [Column("drivingLicenseBack", TypeName = "image")]
    public byte[]? DrivingLicenseBack { get; set; }

    [Column("nationalIDFront", TypeName = "image")]
    public byte[]? NationalIdfront { get; set; }

    [Column("nationalIDBack", TypeName = "image")]
    public byte[]? NationalIdback { get; set; }

    [Column("isactive")]
    public bool Isactive { get; set; }

    public string? Address { get; set; }

    [InverseProperty("Customer")]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

<<<<<<< HEAD

=======
    [InverseProperty("User")]
    public virtual ICollection<CustomerCreditCard> CustomerCreditCards { get; set; } = new List<CustomerCreditCard>();
>>>>>>> Mahmoud-V3

    [ForeignKey("aspNetUserId")]
    [InverseProperty("Customer")]
    public virtual AspNetUser User { get; set; } = null!;
<<<<<<< HEAD

    [InverseProperty("Customer")]
    public virtual ICollection<CustomerRating> CustomerRatings { get => customerRatings; set => customerRatings = value; }

=======
>>>>>>> Mahmoud-V3
}
