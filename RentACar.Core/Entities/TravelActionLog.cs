using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentACar.Core.Entities;

public class TravelActionLog
{
    [Key]
    public int TravelActionLogId { get; set; }

    [Column("customerID")]
    public int CustomerId { get; set; }

    [Required]
    [MaxLength(150)]
    public string CustomerUsername { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ActionType { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Provider { get; set; } = "Booking.com";

    [Required]
    [MaxLength(450)]
    public string ActorAspNetUserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string ActorUserName { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string ActorRole { get; set; } = string.Empty;

    public bool PerformedByEmployee { get; set; }

    [Required]
    public string RequestPayload { get; set; } = string.Empty;

    public string? ResponsePayload { get; set; }

    public bool IsSuccessful { get; set; }

    [MaxLength(512)]
    public string? FailureReason { get; set; }

    [MaxLength(128)]
    public string? ProviderReference { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }
}
