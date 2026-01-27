using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities
{
    [Table("CustomerRatings")]
    public partial class CustomerRating
    {
        [Key]
        [Column("customerRatingID")]
        public int RatingId { get; set; }

        [Column("customerID")]
        public int CustomerId { get; set; }

        [Column("bookingID")]
        public int BookingId { get; set; }

        [Column("stars")]
        public int Stars { get; set; }

        [Column("feedback")]
        public string? Feedback { get; set; }

        [Column("ratingDate")]
        public DateTime RatingDate { get; set; }

        // Navigation to Customer
        [ForeignKey("CustomerId")]
        [InverseProperty("CustomerRatings")]
        public virtual Customer? Customer { get; set; }

        // Navigation to Booking
        [ForeignKey("BookingId")]
        public virtual Booking? Booking { get; set; }
    }
}