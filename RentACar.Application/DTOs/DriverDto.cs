using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RentACar.Application.DTOs;

public class DriverDto
{
    public int DriverId { get; set; }

    [Required]
    [MaxLength(120)]
    public string FullName { get; set; } = null!;

    [Required]
    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string DriverCode { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public decimal? Rating { get; set; }

    public string? LicenseNumber { get; set; }

    public DateOnly? LicenseExpiry { get; set; }

    public string? Languages { get; set; }

    public string? Notes { get; set; }

    [JsonIgnore]
    public string AspNetUserId { get; set; } = string.Empty;
}

public class DriverCreateDto
{
    [Required]
    [MaxLength(120)]
    public string FullName { get; set; } = null!;

    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public string? LicenseNumber { get; set; }

    public DateOnly? LicenseExpiry { get; set; }

    public string? Languages { get; set; }

    public string? Notes { get; set; }
}

public class DriverDisplayDto
{
    public int DriverId { get; set; }
    public string DriverCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
}

public class DriverAvailabilityDto
{
    public int DriverAvailabilityId { get; set; }
    public int DriverId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public bool IsRecurringWeekly { get; set; }
    public bool IsAvailable { get; set; }
}
